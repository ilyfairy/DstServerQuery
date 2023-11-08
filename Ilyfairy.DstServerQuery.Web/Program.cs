//入口点
using Ilyfairy.DstServerQuery;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Requests;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Web;
using Ilyfairy.DstServerQuery.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Settings.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);
if (File.Exists("secrets.json"))
{
    builder.Configuration.AddJsonFile("secrets.json");
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger(); // Run之前生效

builder.Services.AddSerilog((service, loggerConfiguration) =>
{
    //var options = new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly, typeof(Serilog.LoggerConfiguration).Assembly);
    //new LoggerConfiguration().ReadFrom.Configuration(configuration, options); 
    //loggerConfiguration.ReadFrom.Configuration(service.GetRequiredService<IConfiguration>()); // 单文件读取失败

    loggerConfiguration.WriteTo.Async(v => v.Console());
    loggerConfiguration.Enrich.FromLogContext();

    //如果不是开发模式
    if (!builder.Environment.IsDevelopment())
    {
        loggerConfiguration.WriteTo.Async(v => 
            v.File("Logs/log.log", rollingInterval: RollingInterval.Day)
        );
        loggerConfiguration.MinimumLevel.Information();
        loggerConfiguration.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information);
        loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);
        loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning);
    }
#if DEBUG
    loggerConfiguration.MinimumLevel.Debug();
#endif
});

//DbContext
string? sqlType = builder.Configuration.GetValue<string>("SqlType")!;
//使用SqlServer
if (sqlType is "SqlServer" && builder.Configuration.GetConnectionString("SqlServer") is string sqlServerConnection && !string.IsNullOrWhiteSpace(sqlServerConnection))
{
    Log.Logger.Information("使用SqlServer数据库");
    builder.Services.AddSqlServer<DstDbContext>(sqlServerConnection, options =>
    {
        options.MigrationsAssembly("Ilyfairy.DstServerQuery.Web");
    });
}
//使用MySql
else if(sqlType is "MySql" && builder.Configuration.GetConnectionString("MySql") is string mysqlConnection && !string.IsNullOrWhiteSpace(mysqlConnection))
{
    Log.Logger.Information("使用MySql数据库");
    builder.Services.AddMySql<DstDbContext>(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection), options =>
    {
        options.MigrationsAssembly("Ilyfairy.DstServerQuery.Web");
    });
    //builder.Services.AddDbContext<DstDbContext>(options =>
    //{
    //    options.UseMySQL(mysqlConnection);
    //});
}
//使用Sqlite
else if (sqlType is "Sqlite" && builder.Configuration.GetConnectionString("Sqlite") is string sqliteConnection && !string.IsNullOrWhiteSpace(sqliteConnection))
{
    Log.Logger.Information("使用Sqlite数据库");
    builder.Services.AddSqlite<DstDbContext>(sqliteConnection, options =>
    {
        options.MigrationsAssembly("Ilyfairy.DstServerQuery.Web");
    });
}
else
{
    Log.Logger.Information("使用内存数据库");
    builder.Services.AddDbContext<DstDbContext>(v =>
    {
        v.UseInMemoryDatabase("dst");
    });
}
////使用内存数据库
//else if (builder.Configuration.GetConnectionString("InMemory") != null)
//{
//    builder.Services.AddSqlServer<DstDbContext>(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0");
//}
//否则使用Sqlite

builder.Services.AddResponseCompression(); //启用压缩
builder.Services.AddSingleton<HistoryCountManager>();
builder.Services.AddSingleton(builder.Configuration.GetSection("DstConfig").Get<DstWebConfig>()!);
builder.Services.AddSingleton<LobbyDetailsManager>();
builder.Services.AddSingleton<LobbyDetailsManager>();
builder.Services.AddSingleton<DstVersionService>();
builder.Services.AddSingleton<GeoIPService>();
builder.Services.AddSingleton<DstJsonOptions>();
builder.Services.AddSingleton<DstHistoryService>();

//配置跨域请求
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS", policyBuilder =>
        {
            //string[] origins = builder.Configuration.GetSection("CORS:Origins").Get<string[]>() ?? Array.Empty<string>();

            policyBuilder
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true);
        });
});

//版本管理
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0); // 默认api版本
    options.AssumeDefaultVersionWhenUnspecified = true; // 没有指定版本时, 使用默认版本
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.SubstituteApiVersionInUrl = true;
    options.GroupNameFormat = "'v'V"; // v{版本}
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

//添加控制器
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
{
    //默认Json序列化选项
    opt.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
});

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

CultureInfo.CurrentCulture = new CultureInfo("zh-CN");

var app = builder.Build();

app.UseCors("CORS");

// 禁止访问/ 以及用来检测服务器是否正常运行
app.Use(async (v,next) =>
{
    if(v.Request.Path == "" || v.Request.Path == "/")
    {
        v.Response.StatusCode = 404;
        return;
    }
    await next();
});

app.UseResponseCompression();
//app.UseSerilogRequestLogging();


app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHostApplicationBuilder>>();
    
    //数据库迁移
    var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();
    bool isMigration = false;
    if (string.Equals(dbContext.Database.ProviderName,"sqlserver", StringComparison.OrdinalIgnoreCase))
    {
        isMigration = true;
    }
    try
    {
        if (isMigration)
            isMigration = dbContext.Database.GetPendingMigrations().Any();
    }
    catch { }
    if (isMigration)
    {
        await dbContext.Database.MigrateAsync(); //执行迁移
        logger.LogInformation("数据库迁移成功");
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("数据库创建成功");
    }

    //设置Steam代理api
    DepotDownloader.SteamConfig.SetApiUrl(app.Configuration.GetValue<string>("SteampoweredApiProxy") ?? "https://api.steampowered.com/");

    var dstWebConfig = app.Services.GetRequiredService<DstWebConfig>();

    //配置GeoIP
    var geoIPService = app.Services.GetRequiredService<GeoIPService>();
    var dstJsonConverter = app.Services.GetRequiredService<DstJsonOptions>();
    if (app.Configuration.GetValue<string>("GeoLite2Path") is string geoLite2Path)
    {
        geoIPService.Initialize(geoLite2Path);
    }
    dstJsonConverter.DeserializerOptions.Converters.Add(new IPAddressInfoConverter(geoIPService));
    dstJsonConverter.SerializerOptions.Converters.Add(new IPAddressInfoConverter(geoIPService));

    var lobbyManager = app.Services.GetRequiredService<LobbyDetailsManager>();
    await lobbyManager.Start();
    logger.LogInformation("IHostApplicationBuilder Start");

    var dstManager = app.Services.GetRequiredService<DstVersionService>();
    _ = dstManager.RunAsync(dstWebConfig.DstDefaultVersion);

    app.Services.GetRequiredService<DstHistoryService>(); // 确保构造执行
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHostApplicationBuilder>>();
    logger.LogInformation("IHostApplicationBuilder Shutdowning");

    var lobbyManager = app.Services.GetService<LobbyDetailsManager>()!;
    var dstVersion = app.Services.GetService<DstVersionService>()!;
    dstVersion.Dispose();
    lobbyManager.Dispose();

    Log.CloseAndFlush();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => 
    {
        foreach (var description in app.DescribeApiVersions())
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });
}

app.Use(async (context, next) =>
{
    //if (!app.Services.GetRequiredService<LobbyDetailsManager>().Running)
    //{
    //    context.Response.StatusCode = 500;
    //    return;
    //}
    await next();
});

app.MapControllers();

app.Run();

//入口点
using AspNetCoreRateLimit;
using DstDownloaders.Mods;
using DstServerQuery;
using DstServerQuery.EntityFrameworkCore;
using DstServerQuery.EntityFrameworkCore.Model;
using DstServerQuery.Helpers;
using DstServerQuery.Helpers.Converters;
using DstServerQuery.Models.Requests;
using DstServerQuery.Services;
using DstServerQuery.Web;
using DstServerQuery.Web.Helpers;
using DstServerQuery.Web.Helpers.Console;
using DstServerQuery.Web.Models.Configurations;
using DstServerQuery.Web.Services;
using DstServerQuery.Web.Services.TrafficRateLimiter;
using Ilyfairy.DstDownloaders;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
if (File.Exists("secrets.json"))
{
    builder.Configuration.AddJsonFile("secrets.json");
}

bool enabledCommandLine = builder.Configuration.GetSection("EnabledCommandLine").Get<bool?>() is true;

#region Logger
{
    var consoleSink = ControllableConsoleSink.Create();
    consoleSink.Enabled = !enabledCommandLine;
    builder.Services.AddSingleton(consoleSink);

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Sink(consoleSink)
        .CreateBootstrapLogger(); // Run之前生效
}

builder.Services.AddSerilog((service, loggerConfiguration) =>
{
    //var options = new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly, typeof(Serilog.LoggerConfiguration).Assembly);
    //new LoggerConfiguration().ReadFrom.Configuration(configuration, options); 
    //loggerConfiguration.ReadFrom.Configuration(service.GetRequiredService<IConfiguration>()); // 单文件读取失败

    loggerConfiguration.WriteTo.Async(v =>
    {
        v.Sink(service.GetRequiredService<ControllableConsoleSink>());
    });
    loggerConfiguration.Enrich.FromLogContext();

    if (builder.Environment.IsDevelopment())
    {
        loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning);
    }
    else
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
#endregion

#region DbContext
//DbContext
DatabaseType databaseType = builder.Configuration.GetValue<DatabaseType>("SqlType")!;
//使用SqlServer
if (databaseType is DatabaseType.SqlServer)
{
    Log.Logger.Information("使用SqlServer数据库");
    string connectionString = builder.Configuration.GetConnectionString(databaseType.ToString())!;
    builder.Services.AddSqlServer<SqlServerDstDbContext>(connectionString, options =>
    {
        options.MigrationsAssembly("Ilyfairy.DstServerQuery.Web");
    });
    builder.Services.AddScoped<DstDbContext>(v => v.GetRequiredService<SqlServerDstDbContext>());
}
//使用MySql
else if (databaseType is DatabaseType.MySql)
{
    Log.Logger.Information("使用MySql数据库");
    string connectionString = builder.Configuration.GetConnectionString(databaseType.ToString())!;
    builder.Services.AddMySql<MySqlDstDbContext>(connectionString, ServerVersion.AutoDetect(connectionString), options =>
    {
        options.MigrationsAssembly("Ilyfairy.DstServerQuery.Web");
    });
    //builder.Services.AddDbContext<DstDbContext>(options =>
    //{
    //    options.UseMySQL(mysqlConnection);
    //});
    builder.Services.AddScoped<DstDbContext>(v => v.GetRequiredService<MySqlDstDbContext>());
}
//使用Sqlite
else if (databaseType is DatabaseType.Sqlite)
{
    Log.Logger.Information("使用Sqlite数据库");
    string connectionString = builder.Configuration.GetConnectionString(databaseType.ToString())!;
    builder.Services.AddSqlite<SqliteDstDbContext>(connectionString, options =>
    {
        options.MigrationsAssembly("Ilyfairy.DstServerQuery.Web");
    });
    builder.Services.AddScoped<DstDbContext>(v => v.GetRequiredService<SqliteDstDbContext>());
}
//使用PostgreSql
else if (databaseType is DatabaseType.PostgreSql)
{
    Log.Logger.Information("使用PostgreSql数据库");
    string connectionString = builder.Configuration.GetConnectionString(databaseType.ToString())!;
    builder.Services.AddNpgsql<PostgreSqlDstDbContext>(connectionString, options =>
    {
        options.MigrationsAssembly("Ilyfairy.DstServerQuery.Web");
    });
    builder.Services.AddScoped<DstDbContext>(v => v.GetRequiredService<PostgreSqlDstDbContext>());
}
else if (databaseType is DatabaseType.Memory)
{
    Log.Logger.Information("使用内存数据库");
    builder.Services.AddDbContext<MemoryDstDbContext>(v =>
    {
        v.UseInMemoryDatabase("Dst");
    });
    builder.Services.AddScoped<DstDbContext>(v => v.GetRequiredService<MemoryDstDbContext>());
}
else
{
    throw new Exception("unknown database type");
}
////使用内存数据库
//else if (builder.Configuration.GetConnectionString("InMemory") != null)
////{
//    builder.Services.AddSqlServer<DstDbContext>(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0");
//}

builder.Services.AddSqlite<SimpleCacheDatabase>($"Data Source={Path.Join(AppContext.BaseDirectory, "cache.db")}"); // 临时缓存数据库
//builder.Services.AddDatabaseSelector(v =>
//{
//    v.DatabaseType = databaseType;
//});
#endregion

//流量速率限制
builder.Services.AddTrafficLimiter(options =>
{
    options.StatusCode = 429;
    options.MaxQueue = 10000;
    var trafficRate = builder.Configuration.GetRequiredSection("TrafficRateLimit");

    options.TrafficAny = trafficRate.GetSection("Any").Get<TrafficRateLimit[]>() ?? [];
    options.TrafficTargets = trafficRate.GetSection("Targets").Get<Dictionary<string, TrafficRateLimit[]>>() ?? [];
    options.IPHeader = trafficRate.GetSection("IPHeader").Get<string[]>() ?? [];
});

var dstModsFileServiceOptions = builder.Configuration.GetSection("DstModsFileService").Get<DstModsFileServiceOptions>()!;

builder.Services.AddMemoryCache();

builder.Services.AddSingleton(builder.Configuration.GetSection("DstConfig").Get<DstWebConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("Steam").Get<SteamOptions>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("DstVersionService").Get<DstVersionServiceOptions>()!);
builder.Services.AddSingleton(dstModsFileServiceOptions);
builder.Services.AddSingleton<HistoryCountService>();
builder.Services.AddSingleton<LobbyServerManager>();
builder.Services.AddSingleton<DstVersionService>();
builder.Services.AddSingleton<GeoIPService>();
builder.Services.AddSingleton<LobbyDownloader>();
builder.Services.AddHostedService<DstHistoryService>();
builder.Services.AddHostedService<StringCacheService>();
builder.Services.AddHistoryCleanupService(builder.Configuration.GetSection("DstConfig").Get<DstWebConfig>()!.HistoryExpiration);

if (dstModsFileServiceOptions.IsEnabled)
{
    // mods文件服务
    builder.Services.AddSingleton<DstModsFileService>(v =>
    {
        var dst = new DstDownloader(Helper.CreateSteamSession(v));
        if (dstModsFileServiceOptions.FileUrlProxy is { })
        {
            dst.FileUrlProxy = v => new Uri(dstModsFileServiceOptions.FileUrlProxy.Replace("{url}", v.ToString(), StringComparison.OrdinalIgnoreCase));
        }
        return new DstModsFileService(dst, dstModsFileServiceOptions.RootPath);
    });
    builder.Services.AddHostedService<DstModsFileHosedService>();
}

//IP速率限制
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

////速率限制
//builder.Services.AddRateLimiter(_ => _
//    .AddFixedWindowLimiter(policyName: "fixed", options =>
//    {
//        options.PermitLimit = builder.Configuration.GetSection("LimitCount").Get<int>();
//        options.Window = TimeSpan.FromSeconds(builder.Configuration.GetSection("LimitTime").Get<int>());
//        options.QueueProcessingOrder = QueueProcessingOrder.NewestFirst;
//        options.QueueLimit = 2;
//    }));

//配置压缩
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(v => v.Level = System.IO.Compression.CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(v => v.Level = System.IO.Compression.CompressionLevel.Optimal);

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
    opt.JsonSerializerOptions.TypeInfoResolverChain.Add(DstRawJsonContext.Default);
    opt.JsonSerializerOptions.TypeInfoResolverChain.Add(DstLobbyInfoJsonContext.Default);

    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opt.JsonSerializerOptions.Converters.Add(new ReadOnlyMemoryCharJsonConverter());
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
    opt.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var currentXmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(currentXmlFilePath))
    {
        options.IncludeXmlComments(currentXmlFilePath);
    }
    var queryXmlFilePath = Path.Combine(AppContext.BaseDirectory, "Ilyfairy.DstServerQuery.xml");
    if (File.Exists(queryXmlFilePath))
    {
        options.IncludeXmlComments(queryXmlFilePath);
    }
});

builder.Services.AddSingleton<CommandService>();

CultureInfo.CurrentCulture = new CultureInfo("zh-CN");

var app = builder.Build();



//`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//



app.UseCors("CORS");

// 禁止访问/  或用来检测服务器是否正常运行
app.Use(async (v, next) =>
{
    if (v.Request.Path == "" || v.Request.Path == "/")
    {
        v.Response.StatusCode = 204;
        return;
    }
    await next();
});

//请求流量限制
app.UseTrafficLimiter(async (context, v2, next) =>
{
    Log.Warning("流量速率限制  IP:{IP}  Path:{Path}", v2.IP, context.Request.Path.ToString());
    await context.Response.WriteAsync("""{"Code":429,"Error":"Too Many Requests"}""");
});

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseResponseCompression();
app.UseSerilogRequestLogging();

//app.UseRateLimiter(); // 速率限制
app.UseIpRateLimiting(); // IP速率限制

app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHostApplicationBuilder>>();
    logger.LogInformation("IHostApplicationBuilder Start");

    //键值对缓存数据库
    var cache = scope.ServiceProvider.GetRequiredService<SimpleCacheDatabase>();
    cache.EnsureInitialize();

    //数据库迁移
    var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();
    bool isMigration = false;
    try
    {
        isMigration = dbContext.Database.GetPendingMigrations().Any();
    }
    catch { }
    if (isMigration)
    {
        dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(100));
        await dbContext.Database.MigrateAsync(); //执行迁移
        logger.LogInformation("数据库迁移成功");
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("数据库创建成功");
    }

    var dstWebConfig = app.Services.GetRequiredService<DstWebConfig>();
    var steamOptions = app.Services.GetRequiredService<SteamOptions>();
    var dstVersionServiceOptions = app.Services.GetRequiredService<DstVersionServiceOptions>();
    var dstModsFileServiceOptions = app.Services.GetRequiredService<DstModsFileServiceOptions>();

    //配置GeoIP
    if (app.Configuration.GetValue<string>("GeoLite2Path") is string geoLite2Path)
    {
        var geoIPService = app.Services.GetRequiredService<GeoIPService>();
        geoIPService.Initialize(geoLite2Path);
        DstConverterHelper.GeoIPService = geoIPService;
    }

    //启动服务管理器
    var lobbyServerManager = app.Services.GetRequiredService<LobbyServerManager>();
    await lobbyServerManager.Start();

    //饥荒版本获取服务
    var dstVersionService = app.Services.GetRequiredService<DstVersionService>();
    dstVersionService.DstDownloaderFactory = () =>
    {
        return new DstDownloader(Helper.CreateSteamSession(app.Services));
    };
    var defaultVersion = cache.Get<long?>("DstVersion") ?? dstVersionServiceOptions.DefaultVersion;
    _ = dstVersionService.RunAsync(defaultVersion);
    var dstVersionDatabase = app.Services.CreateScope().ServiceProvider.GetRequiredService<SimpleCacheDatabase>(); // 不销毁
    dstVersionService.VersionUpdated += (sender, version) =>
    {
        dstVersionDatabase["DstVersion"] = version;
    };

});

app.Lifetime.ApplicationStopped.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("IHostApplicationBuilder Shutdowning");

    var lobbyManager = app.Services.GetRequiredService<LobbyServerManager>();
    var dstVersion = app.Services.GetRequiredService<DstVersionService>()!;
    var dstModsService = app.Services.GetService<DstModsFileService?>();
    dstVersion.Dispose();
    lobbyManager.Dispose();
    dstModsService?.Dispose();

    Log.CloseAndFlush();
});

if (app.Environment.IsDevelopment())
{
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.InjectJavascript("/swagger/swagger_ext.js");
    foreach (var description in app.DescribeApiVersions().Reverse())
    {
        var url = $"/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        options.SwaggerEndpoint(url, name);
    }
});

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

if (enabledCommandLine)
{
    await app.StartAsync();
    await app.Services.GetRequiredService<CommandService>().RunCommandLoopAsync();
}
else
{
    app.Run();
}

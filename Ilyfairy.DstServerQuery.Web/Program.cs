//入口点
using Ilyfairy.DstServerQuery;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Requests;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
if (File.Exists("secrets.json"))
{
    builder.Configuration.AddJsonFile("secrets.json");
}

//DbContext
if (builder.Configuration.GetConnectionString("SqlServer") is string sqlServerConnection && !string.IsNullOrWhiteSpace(sqlServerConnection))
{
    builder.Services.AddSqlServer<DstDbContext>(sqlServerConnection);
}
else
{
    LogManager.GetLogger("Initialize").Warn("没有检测到SqlServer连接字符串, 将使用内存数据库");
    builder.Services.AddDbContext<DstDbContext>(options =>
    {
        options.UseInMemoryDatabase("Dst");
    });
}

builder.Services.AddResponseCompression(); //启用压缩
builder.Services.AddSingleton<HistoryCountManager>();
builder.Services.AddSingleton(builder.Configuration.GetSection("Requests").Get<RequestRoot>()!);
builder.Services.AddSingleton<LobbyDetailsManager>();
builder.Services.AddSingleton<LobbyDetailsManager>();
builder.Services.AddSingleton<DstVersionService>();
builder.Services.AddSingleton<GeoIPService>();
builder.Services.AddSingleton<DstJsonOptions>();

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
app.UseResponseCompression();


app.Lifetime.ApplicationStarted.Register(async () =>
{
    //创建数据库表
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DstDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    //初始化
    var webLog = LogManager.GetLogger("Lifetime");
    var lobbyManager = app.Services.GetRequiredService<LobbyDetailsManager>();
    var dstManager = app.Services.GetRequiredService<DstVersionService>();
    var historyCountManager = app.Services.GetRequiredService<HistoryCountManager>();
    //var reqs = app.Services.GetRequiredService<RequestRoot>();
    var geoIPService = app.Services.GetRequiredService<GeoIPService>();
    var dstJsonConverter = app.Services.GetRequiredService<DstJsonOptions>();

    //设置Steam代理api
    DepotDownloader.SteamConfig.SetApiUrl(app.Configuration.GetValue<string>("SteampoweredApiProxy") ?? "https://api.steampowered.com/");

    //配置GeoIP
    if (app.Configuration.GetValue<string>("GeoLite2Path") is string geoLite2Path)
    {
        geoIPService.Initialize(geoLite2Path);
    }
    dstJsonConverter.DeserializerOptions.Converters.Add(new IPAddressInfoConverter(geoIPService));
    dstJsonConverter.SerializerOptions.Converters.Add(new IPAddressInfoConverter(geoIPService));

    //服务器更新回调
    lobbyManager.Updated += (sender, e) =>
    {
        if (e.Data.Count == 0) return;
        var data = e.Data;
        if (e.IsDetailed)
        {
            historyCountManager.Add(data, e.UpdatedDateTime);
        }
    };

    await lobbyManager.Start();
    webLog.Info("<Start>");

    dstManager.RunAsync(app.Configuration.GetValue<long?>("DstDefaultVersion"));
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    var webLog = LogManager.GetLogger("Lifetime");
    webLog.Info("<Shutdowning>");

    var lobbyManager = app.Services.GetService<LobbyDetailsManager>()!;
    var dstVersion = app.Services.GetService<DstVersionService>()!;
    dstVersion.Dispose();
    lobbyManager.Dispose();
    webLog.Info("<Shutdown>");
    LogManager.Shutdown();
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

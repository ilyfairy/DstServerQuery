//入口点
using Ilyfairy.DstServerQuery;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.LobbyJson.Converter;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Requests;
using Ilyfairy.DstServerQuery.Services;
using Ilyfairy.DstServerQuery.Utils;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
if (File.Exists("secrets.json"))
{
    builder.Configuration.AddJsonFile("secrets.json");
}
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
});

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
builder.Services.AddSingleton<DstVersionGetter>();
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
    var dstManager = app.Services.GetRequiredService<DstVersionGetter>();
    var historyCountManager = app.Services.GetRequiredService<HistoryCountManager>();
    //var reqs = app.Services.GetRequiredService<RequestRoot>();
    var geoIPService = app.Services.GetRequiredService<GeoIPService>();
    var dstJsonConverter = app.Services.GetRequiredService<DstJsonOptions>();


    DepotDownloader.SteamConfig.SetApiUrl(app.Configuration.GetValue<string>("SteampoweredApiProxy") ?? "https://api.steampowered.com/");

    if (app.Configuration.GetValue<string>("GeoLite2Path") is string geoLite2Path)
    {
        geoIPService.Initialize(geoLite2Path);
    }
    dstJsonConverter.DeserializerOptions.Converters.Add(new IPAddressInfoConverter(geoIPService));
    dstJsonConverter.SerializerOptions.Converters.Add(new IPAddressInfoConverter(geoIPService));

    //服务器更新回调
    lobbyManager.Updated += (sender, e) =>
    {
        if (!e.IsDownloadCompleted || e.Data.Count == 0) return;
        var data = e.Data;
        historyCountManager.Add(data, e.UpdatedDateTime);
        GC.Collect();
    };

    await lobbyManager.Start();
    webLog.Info("<Start>");

    dstManager.Start(app.Configuration.GetValue<long?>("DstDefaultVersion"));
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    var webLog = LogManager.GetLogger("Web");
    webLog.Info("<Shutdowning>");

    var lobbyManager = app.Services.GetService<LobbyDetailsManager>()!;
    var dstVersion = app.Services.GetService<DstVersionGetter>()!;
    dstVersion.Abort();
    lobbyManager.Dispose();
    webLog.Info("<Shutdown>");
    LogManager.Shutdown();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Use(async (context, next) =>
{
    //context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    //context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
    //context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
    //context.Response.Headers.Add("Access-Control-Allow-Credentials", "false");
    if (!app.Services.GetService<LobbyDetailsManager>()!.Running)
    {
        context.Response.StatusCode = 500;
        return;
    }
    await next();
});

app.MapControllers();

app.Run();

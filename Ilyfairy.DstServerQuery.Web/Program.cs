//入口点
using Ilyfairy.DstServerQuery;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Requests;
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
builder.Services.AddResponseCompression(); //启用压缩

//builder.Services.AddSqlServer<DstDbContext>(builder.Configuration.GetConnectionString("SqlServer")!);
if (builder.Configuration.GetConnectionString("SqlServer") is string sqlServer)
{
    builder.Services.AddSingleton(new HistoryCountManager(() => new DstDbContext(new DbContextOptionsBuilder<DstDbContext>()
            .UseSqlServer(sqlServer).Options)));
}
else
{
    builder.Services.AddSingleton(new HistoryCountManager(null));
    LogManager.GetLogger("Initialize").Warn("没有使用SqlServer");
}

builder.Services.AddSingleton(builder.Configuration.GetSection("Requests").Get<RequestRoot>()!);
builder.Services.AddSingleton<LobbyDetailsManager>()
    .AddSingleton<DstVersionGetter>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

CultureInfo.CurrentCulture = new CultureInfo("zh-cn");
 
var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    var webLog = LogManager.GetLogger("Lifetime");
    var lobbyManager = app.Services.GetService<LobbyDetailsManager>()!;
    var dstManager = app.Services.GetService<DstVersionGetter>()!;
    var historyCountManager = app.Services.GetService<HistoryCountManager>()!;

    var reqs = app.Services.GetService<RequestRoot>()!;

    DepotDownloader.SteamConfig.SetApiUrl(app.Configuration.GetValue<string>("SteampoweredApiProxy") ?? "https://api.steampowered.com/");

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

    dstManager.Start(app.Configuration.GetValue<long>("DstDefaultVersion"));
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
    if (app.Configuration.GetValue<string>("GeoLite2DebugPath") is string geoLite2DebugPath)
    {
        GeoIPManager.Initialize(geoLite2DebugPath);
    }
}

app.UseResponseCompression();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
    if (!app.Services.GetService<LobbyDetailsManager>()!.Running)
    {
        context.Response.StatusCode = 500;
        return;
    }
    await next();
});

app.MapControllers();

app.Run();

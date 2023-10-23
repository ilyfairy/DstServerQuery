using Asp.Versioning;
using Ilyfairy.DstServerQuery;
using Ilyfairy.DstServerQuery.EntityFrameworkCore;
using Ilyfairy.DstServerQuery.LobbyJson;
using Ilyfairy.DstServerQuery.Models;
using Ilyfairy.DstServerQuery.Models.Entities;
using Ilyfairy.DstServerQuery.Models.LobbyData;
using Ilyfairy.DstServerQuery.Utils;
using Ilyfairy.DstServerQuery.Web;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Net.Mime;
using System.Text.Json;

namespace Ilyfairy.DstServerQuery.Web.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("[controller]/v{version:apiVersion}")]
public partial class ApiController : ControllerBase
{
    private static readonly Logger _logger = LogManager.GetLogger("ApiController");

    private readonly LobbyDetailsManager lobbyDetailsManager;
    private readonly HistoryCountManager historyCountManager;
    private readonly DstJsonOptions dstJsonOptions;

    //版本获取, 大厅服务器管理器, 大厅服务器历史房间数量管理器
    public ApiController(LobbyDetailsManager lobbyDetailsManager, HistoryCountManager historyCountManager, DstJsonOptions dstJsonOptions)
    {
        this.lobbyDetailsManager = lobbyDetailsManager;
        this.historyCountManager = historyCountManager;
        this.dstJsonOptions = dstJsonOptions;
        
    }

}

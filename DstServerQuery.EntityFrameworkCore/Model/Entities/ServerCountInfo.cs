using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.EntityFrameworkCore.Models.Entities;

/// <summary>
/// 服务器数量信息
/// </summary>
public record ServerCountInfo
{
    [Key, JsonIgnore]
    public int Id { get; set; }

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//
    public DateTimeOffset UpdateDate { get; set; }
    public int AllServerCount { get; set; }
    public int AllPlayerCount { get; set; }
    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//
    public int SteamServerCount { get; set; }
    public int WeGameServerCount { get; set; }
    public int PlayStationServerCount { get; set; }
    public int XboxServerCount { get; set; }
    public int? SwitchServerCount { get; set; }
    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//
    public int SteamPlayerCount { get; set; }
    public int WeGamePlayerCount { get; set; }
    public int PlayStationPlayerCount { get; set; }
    public int XboxPlayerCount { get; set; }
    public int? SwitchPlayerCount { get; set; }

}

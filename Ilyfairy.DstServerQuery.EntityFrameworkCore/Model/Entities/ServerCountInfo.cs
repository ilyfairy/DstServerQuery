﻿using System.ComponentModel.DataAnnotations;

namespace Ilyfairy.DstServerQuery.Models.Entities;

/// <summary>
/// 服务器个数情况
/// </summary>
public class ServerCountInfo
{
    [Key]
    public int Id { get; set; }

    //`(*>﹏<*)′=================================== 一条华丽的分割线 ===================================`(*>﹏<*)′//
    public DateTime UpdateDate { get; set; }
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

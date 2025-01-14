﻿using DstServerQuery.Models.Lobby.Interfaces.V2;

namespace DstServerQuery.Web.Models.Http;

/// <summary>
/// List响应结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListResponse<T> : ResponseBase where T : ILobbyServerV2
{
    /// <summary>
    /// Http开始响应的时间
    /// </summary>
    public DateTimeOffset DateTime { get; set; }

    /// <summary>
    /// 数据最后更新时间
    /// </summary>
    public DateTimeOffset LastUpdate { get; set; }

    /// <summary>
    /// 当前页个数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 所有个数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页索引
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 最大页索引
    /// </summary>
    public int MaxPageIndex { get; set; }

    /// <summary>
    /// 服务器列表
    /// </summary>
    public IEnumerable<T> List { get; set; } = Array.Empty<T>();
}


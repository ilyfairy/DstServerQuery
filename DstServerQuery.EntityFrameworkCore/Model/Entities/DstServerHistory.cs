﻿using DstServerQuery.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DstServerQuery.EntityFrameworkCore.Model.Entities;

/// <summary>
/// 服务器, 保存了几乎不可变的字段
/// </summary>
[Index(nameof(Id), nameof(UpdateTime), nameof(Name))]
public class DstServerHistory
{
    /// <summary>
    /// 主键<br/>RowId
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required string Id { get; set; } = null!;

    public string Name { get; set; }
    public string IP { get; set; }
    public int Port { get; set; }
    public string Host { get; set; }
    public DateTimeOffset UpdateTime { get; set; }
    public Platform Platform { get; set; }
    public string? GameMode { get; set; }
    public string? Intent { get; set; }

    [JsonIgnore]
    public ICollection<DstServerHistoryItem> Items { get; set; } = [];
}


[Index(nameof(Id), nameof(DateTime))]
public class DstServerHistoryItem
{
    [Key]
    public long Id { get; set; }

    public string? Season { get; set; }
    public int PlayerCount { get; set; }

    public DateTimeOffset DateTime { get; set; }

    public string ServerId { get; set; }
    public DstServerHistory Server { get; set; }


    public bool IsDetailed { get; set; }
    public DstDaysInfo? DaysInfo { get; set; }
    public long? DaysInfoId { get; set; }
    public ICollection<DstPlayer>? Players { get; set; }

    public int GetPlayerCount() => Players?.Count ?? PlayerCount;
}

public class DstDaysInfo
{
    [Key]
    [JsonIgnore]
    public long Id { get; set; }

    /// <summary>
    /// 当前天数
    /// </summary>
    public int Day { get; set; }

    /// <summary>
    /// 当前季节已过去天数
    /// </summary>
    public int DaysElapsedInSeason { get; set; }

    /// <summary>
    /// 当前季节剩余天数
    /// </summary>
    public int DaysLeftInSeason { get; set; }

    public int TotalDaysSeason => DaysElapsedInSeason + DaysLeftInSeason;

    [JsonIgnore]
    public DstServerHistoryItem ServerItem { get; set; } = null!;

    [return: NotNullIfNotNull(nameof(lobbyDaysInfo))]
    public static DstDaysInfo? FromLobby(LobbyDaysInfo? lobbyDaysInfo)
        => lobbyDaysInfo is null ? null : new() { Day = lobbyDaysInfo.Day, DaysElapsedInSeason = lobbyDaysInfo.DaysElapsedInSeason, DaysLeftInSeason = lobbyDaysInfo.DaysLeftInSeason };
}


public class HistoryServerItemPlayer
{
    public DstPlayer Player { get; set; }
    public string PlayerId { get; set; }

    public DstServerHistoryItem HistoryServerItem { get; set; }
    public long HistoryServerItemId { get; set; }
}
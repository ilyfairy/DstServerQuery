using DstServerQuery.EntityFrameworkCore.Model.Entities;
using System.Text.Json.Serialization;

namespace DstServerQuery.Web.Models;

public record ServerHistoryItem
{
    public string? Season { get; set; }
    public int PlayerCount { get; set; }
    public DateTimeOffset DateTime { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DstDaysInfo? DaysInfo { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<DstPlayer>? Players { get; set; }

    public static ServerHistoryItem From(DstServerHistoryItem item)
    {
        return new()
        {
            Season = item.Season,
            DateTime = item.DateTime,
            PlayerCount = item.PlayerCount,
            DaysInfo = item.IsDetailed ? item.DaysInfo : null,
            Players = item.IsDetailed ? item.Players : null,
        };
    }
}
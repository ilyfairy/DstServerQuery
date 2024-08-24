using DstServerQuery.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DstServerQuery.EntityFrameworkCore.Model.Entities;

/// <summary>
/// 服务器玩家
/// </summary>
[Index(nameof(Name), nameof(Platform))]
public class DstPlayer
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required string Id { get; set; }
    public required string Name { get; set; }
    public Platform Platform { get; set; }

    /// <summary>
    /// 这个玩家存在哪些服务器中存在过
    /// </summary>
    [JsonIgnore]
    public ICollection<DstServerHistoryItem> ServerHistoryItems { get; set; } = [];
}

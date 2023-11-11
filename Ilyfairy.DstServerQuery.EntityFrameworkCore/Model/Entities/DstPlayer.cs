using Ilyfairy.DstServerQuery.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ilyfairy.DstServerQuery.EntityFrameworkCore.Model.Entities;

/// <summary>
/// 服务器玩家
/// </summary>
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

using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Ilyfairy.DstServerQuery.Models.Entities;

/// <summary>
/// 用户请求
/// </summary>
public class UserRequestRecord
{
    [Key]
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Path { get; set; }
    public string? IP { get; set; }
    public string? UserAgent { get; set; }
}

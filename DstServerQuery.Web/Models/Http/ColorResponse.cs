namespace DstServerQuery.Web.Models.Http;

/// <summary>
/// 颜色键值对响应
/// </summary>
public class ColorResponse : ResponseBase
{
    /// <summary>
    /// Name:FFFFFF
    /// </summary>
    public required Dictionary<string, string> Colors { get; set; }
}
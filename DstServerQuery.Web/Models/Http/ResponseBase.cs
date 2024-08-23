using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Ilyfairy.DstServerQuery.Web.Models;

/// <summary>
/// 响应
/// </summary>
public class ResponseBase
{
    /// <summary>
    /// 响应码
    /// </summary>
    public int Code { get; set; } = 200;

    /// <summary>
    /// 当Code不等于200时的错误消息
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    public static JsonResult From(ResponseBase model, object? serializerSettings = null)
    {
        return new JsonResult(model, serializerSettings) { StatusCode = model.Code };
    }

    public static JsonResult From(ResponseBase model, int statusCode, object? serializerSettings = null)
    {
        model.Code = statusCode;
        return new JsonResult(model, serializerSettings) { StatusCode = statusCode };
    }

    public static JsonResult NotFound(string errorMessage = "Not Found")
    {
        return new JsonResult(new ResponseBase() { Code = 404, Error = errorMessage }) { StatusCode = 404 };
    }

    public static JsonResult BadRequest(string errorMessage = "Bad Request")
    {
        return new JsonResult(new ResponseBase() { Code = 400, Error = errorMessage }) { StatusCode = 400 };
    }

    public JsonResult ToJsonResult(object? serializerSettings = null)
    {
        return new JsonResult(this, serializerSettings) { StatusCode = Code };
    }
}

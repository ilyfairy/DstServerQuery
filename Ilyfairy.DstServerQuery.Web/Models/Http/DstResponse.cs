using Microsoft.AspNetCore.Mvc;

namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class DstResponse(object model) : JsonResult(model)
{
    public static DstResponse NotFound(string errorMessage = "Not Found")
    {
        return new DstResponse(new ErrorResponse() { Error = errorMessage }) { StatusCode = 404 };
    }
    public static DstResponse BadRequest(string errorMessage = "Bad Request")
    {
        return new DstResponse(new ErrorResponse() { Error = errorMessage }) { StatusCode = 400 };
    }
}


file class ErrorResponse
{
    public required string Error { get; set; }
}
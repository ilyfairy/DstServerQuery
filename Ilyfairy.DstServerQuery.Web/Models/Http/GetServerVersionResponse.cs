namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class GetServerVersionResponse : ResponseBase
{
    /// <summary>
    /// 服务器版本
    /// </summary>
    public long? Version { get; set; }

    public GetServerVersionResponse(long? version)
    {
        Version = version;
        if (version is null)
        {
            Code = 503;
            Error = "Service Unavailable";
        }
    }
}

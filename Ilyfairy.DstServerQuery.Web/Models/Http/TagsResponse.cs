namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class TagsResponse(IEnumerable<TagsResponse.ServerTag> tags) : ResponseBase
{
    public IEnumerable<ServerTag> Tags { get; } = tags;

    public record ServerTag(string Tag, int Count);
}

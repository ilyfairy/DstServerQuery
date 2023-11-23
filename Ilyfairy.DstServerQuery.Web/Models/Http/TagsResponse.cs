namespace Ilyfairy.DstServerQuery.Web.Models.Http;

public class TagsResponse(IEnumerable<TagsResponse.Tag> tags) : ResponseBase
{
    public IEnumerable<Tag> Tags { get; } = tags;

    public record Tag(string Prefab, int Count);
}

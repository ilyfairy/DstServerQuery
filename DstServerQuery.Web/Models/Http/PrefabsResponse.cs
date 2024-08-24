namespace DstServerQuery.Web.Models.Http;

public class PrefabsResponse(IEnumerable<PrefabsResponse.PlayerPrefab> prefabs) : ResponseBase
{
    public IEnumerable<PlayerPrefab> Prefabs { get; } = prefabs;

    public record PlayerPrefab(string Prefab, int Count);
}

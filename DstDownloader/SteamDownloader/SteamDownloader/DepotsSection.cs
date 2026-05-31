using SteamKit2;

namespace SteamDownloader;

public class DepotsSection
{
    public uint AppId { get; set; }
    public Dictionary<string, Branch> Branches { get; set; } = [];
    public Dictionary<uint, DepotInfo> DepotsInfo { get; set; } = [];

    public DepotsSection(uint appId, KeyValue depotsSection)
    {
        AppId = appId;
        foreach (var item in depotsSection.Children)
        {
            switch (item.Name)
            {
                case "branches":
                    foreach (var branch in item.Children)
                    {
                        Branches[branch.Name!] = new Branch(branch);
                    }
                    break;
                case { } when uint.TryParse(item.Name, out var depotId):
                    DepotsInfo[depotId] = new DepotInfo(item);
                    break;
                default:

                    break;
            }
        }
    }

    public IEnumerable<DepotInfo> Where(Func<DepotInfo,bool> func)
    {
        return DepotsInfo.Select(v => v.Value).Where(func);
    }

    public record Branch
    {
        public string Name { get; set; }
        public string? BuildId { get; set; }
        public bool PasswordRequired { get; set; }
        public DateTimeOffset? UpdateTime { get; set; }

        public Branch(KeyValue keyValue)
        {
            Name = keyValue.Name!;
            foreach (var item in keyValue.Children)
            {
                switch (item.Name)
                {
                    case "buildid":
                        BuildId = item.Value!;
                        break;
                    case "timeupdated":
                        UpdateTime = DateTimeOffset.FromUnixTimeSeconds(item.AsLong());
                        break;
                    case "pwdrequired":
                        PasswordRequired = item.Value is "1";
                        break;
                }
            }
        }
    }

    public record DepotInfo
    {
        public uint DepotId { get; set; }
        public Config? Config { get; set; }
        public uint? DepotFromApp { get; set; }
        public bool IsSharedInstall { get; set; }
        public Dictionary<string, EncryptedManifest> EncryptedManifests { get; set; } = new();
        public Dictionary<string, Manifest> Manifests { get; set; } = new();

        public DepotInfo(KeyValue appSection)
        {
            DepotId = uint.Parse(appSection.Name!);
            foreach (var item in appSection.Children)
            {
                switch (item.Name)
                {
                    case "config":
                        Config = new Config(item);
                        break;
                    case "depotfromapp":
                        DepotFromApp = uint.Parse(item.Value!);
                        break;
                    case "sharedinstall":
                        IsSharedInstall = item.Value is "1";
                        break;
                    case "manifests":
                        foreach (var manifest in item.Children)
                        {
                            Manifests[manifest.Name!] = new Manifest(manifest);
                        }
                        break;
                    case "encryptedmanifests":
                        foreach (var manifest in item.Children)
                        {
                            EncryptedManifests[manifest.Name!] = new EncryptedManifest(manifest);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public record Manifest
    {
        public string BranchName { get; set; }
        public ulong ManifestId { get; set; }
        public long Size { get; set; }
        public long Download { get; set; }

        public Manifest(KeyValue manifestSection)
        {
            BranchName = manifestSection.Name!;
            foreach (var item in manifestSection.Children)
            {
                switch (item.Name)
                {
                    case "gid":
                        ManifestId = ulong.Parse(item.Value!);
                        break;
                    case "size":
                        Size = long.Parse(item.Value!);
                        break;
                    case "download":
                        Download = long.Parse(item.Value!);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public record EncryptedManifest
    {
        public string BranchName { get; set; }
        public string ManifestId { get; set; } = null!;
        public string Size { get; set; } = null!;
        public string Download { get; set; } = null!;

        public EncryptedManifest(KeyValue encryptedmanifestSection)
        {
            BranchName = encryptedmanifestSection.Name!;
            foreach (var item in encryptedmanifestSection.Children)
            {
                switch (item.Name)
                {
                    case "gid":
                        ManifestId = item.Value!;
                        break;
                    case "size":
                        Size = item.Value!;
                        break;
                    case "download":
                        Download = item.Value!;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public record Config
    {
        public OS Oslist { get; set; }

        public Config(KeyValue configSection)
        {
            foreach (var item in configSection.Children)
            {
                switch (item.Name)
                {
                    case "oslist":
                        Oslist = item.Value switch
                        {
                            "windows" => OS.Windows,
                            "linux" => OS.Linux,
                            "macos" => OS.MacOS,
                            _ => OS.Unknow,
                        };
                        break;
                }
            }
        }
    }

    public enum OS
    {
        Unknow = 0,
        Windows, Linux, MacOS
    }
}

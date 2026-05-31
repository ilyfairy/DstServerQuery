using SteamKit2;

namespace SteamDownloader.Helpers;

public static class PICSProductInfoExtensions
{

    public static KeyValue? GetProductInfoSection(this SteamApps.PICSProductInfoCallback.PICSProductInfo appInfo, EAppInfoSection section)
    {
        string sectionKey = section switch
        {
            EAppInfoSection.Common => "common",
            EAppInfoSection.Extended => "extended",
            EAppInfoSection.Config => "config",
            EAppInfoSection.Depots => "depots",
            EAppInfoSection.Install => "install",
            EAppInfoSection.UFS => "ufs",
            EAppInfoSection.Localization => "localization",
            _ => throw new NotSupportedException(),
        };

        var secion = appInfo.KeyValues.Children.FirstOrDefault(v => v.Name == sectionKey);
        return secion;
    }

    public static DepotsSection GetProductInfoDepotsSection(this SteamApps.PICSProductInfoCallback.PICSProductInfo appInfo)
    {
        var depots = GetProductInfoSection(appInfo, EAppInfoSection.Depots)!;
        return new DepotsSection(appInfo.ID, depots);
    }

}

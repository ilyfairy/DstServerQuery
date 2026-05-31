# SteamDownloader

[SteamKit](https://github.com/SteamRE/SteamKit/)的封装


## 使用:

```cs
using SteamSession steam = new();

// 连接并登录, 需要在控制台中输入邮件发送的验证码,   或者使用匿名登录LoginAnonymousAsync()
await steam.Authentication.LoginAsync("username123", "password456", true);

// 保存登录Token, 下次可以使用`steam.Authentication.LoginFromAccessTokenAsync("username123", token)`登录而无需输入邮件验证码
File.WriteAllText("token.txt", steam.Authentication.AccessToken);

uint appId = 343050;
// 通过AppId获取AppInfo, 一个App中包含了很多仓库
var appInfo = await steam.GetAppInfoAsync(appId);
var depotsContent = steam.GetAppInfoDepotsSection(appInfo);

// 获取Windows平台的仓库
var depots = depotsContent.Where(v => v.Config?.Oslist is DepotsContent.OS.Windows or null);

//遍历所有仓库, 排除没有分支的仓库, 每个仓库有很多分支, 例如公共分支, 开发分支
foreach (var depot in depots.Where(v => v.Manifests.Count > 0))
{
    // 获取public分支的仓库清单, 包含了文件列表等信息
    var manifest = await steam.GetDepotManifestAsync(appId, depot.DepotId, depot.Manifests["public"].ManifestId);

    // 下载清单的所有文件到指定目录
    await steam.DownloadDepotManifestToDirectoryAsync("GameDir", depot.DepotId, manifest);
}
```
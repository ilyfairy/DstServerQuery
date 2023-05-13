# DstServerQuery
饥荒联机版服务器查询 [dstserverlist.top](https://dstserverlist.top)  

从Klei服务器下载所有服务器数据进行查询  

## 生成

运行`publish-win.bat`发布项目  
然后打开`Ilyfairy.DstServerQuery.Web.exe`
接下载访问`http://127.0.0.1:3000/api/list`


## 配置

配置文件是[appsettings.json](Ilyfairy.DstServerQuery.Web/appsettings.json)

### Token
需要到找klei去申请一个token, 否则是不能获取详细信息的, 比如玩家信息

### 代理(可选)
DstDetailsProxyUrl 是一个批量请求详细数据的代理服务器的Url  
它相当于是`https://lobby-v2-cdn.klei.com`代理版本, 请自己搭建代理服务器  
`body`将会传入类似`[{"RowId":"KU_XXXXXXXX","Region":"v2-ap-east-1"}]`的列表

## 接口

- `/api/server/version` 获取饥荒联机版最新版本号  
- `/api/list` 服务器列表  
- `/api/details/{id}` 通过RowId服务器详细信息

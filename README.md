# DstServerQuery
饥荒联机版服务器查询

从Klei服务器下载所有服务器数据进行查询

## 使用

访问 [dstserverlist.top](https://dstserverlist.top)  

或者  

运行`publish.bat`发布项目  
然后打开`Ilyfairy.DstServerQuery.Web`
接下载访问`http://127.0.0.1:3000/api/list`

## Code

### 代理
DstDetailsProxyUrl 是一个批量请求详细数据的代理服务器的Url  
它相当于是`https://lobby-v2-cdn.klei.com`代理版本, 请自己搭建代理服务器  
POST https://api.com/path/{1}  
`{0}` 是Region将被传入`us-east-1` `ap-east-1`等  
`body` 将会传入一个rowid的数组  

## 接口

`/api/server/version` 获取饥荒联机版最新版本号  
`/api/list` 服务器列表  
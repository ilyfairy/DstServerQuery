# DstServerQuery
饥荒联机版服务器查询

从Klei服务器下载所有服务器数据进行查询

## 使用

运行`publish.bat`发布项目  
然后打开`Ilyfairy.DstServerQuery.Web`
接下载访问`http://127.0.0.1:3000/api/list`

## Code

### 代理
DstDetailsProxyUrl 是一个批量请求详细数据的代理服务器的Url  
它相当于是`https://lobby-v2-cdn.klei.com`代理版本, 请自己搭建代理服务器  
并且它的返回数据被修改过, 不是`{"GET":[data1,data2]}`, 而是`{"rowid1": {data1}, "rowid2": {data2}}`  
POST https://api.com/path/{0}/{1}  
`{0}` 将被传入new或者old  
`{1}` 将被传入Region, eu sing等
`body` 将会传入一个rowid的数组

## 接口

`/api/server/version` 获取饥荒联机版最新版本号  
`/api/list` 服务器列表  
# DstServerQuery
服务器查询, 玩家查询, 服务器的历史记录, Mods查询  

从Klei服务器下载所有服务器数据进行查询  

网站: [dstserverlist.top](https://dstserverlist.top)  

## 构建

运行`publish-win.bat`发布项目  
然后打开`Ilyfairy.DstServerQuery.Web.exe`
接下载访问`http://127.0.0.1:3000/api/list`


## 配置

配置文件是[appsettings.json](Ilyfairy.DstServerQuery.Web/appsettings.json)

### 数据库

数据库用来储存服务器的历史记录  
支持SqlServer,MySql,Sqlite,PostgreSql  
并且数据库的排序规则`Collation`需要区分大小写  
如果是MySql,则首次需要执行`SET GLOBAL local_infile = true;`  

### Token
需要开服的Token, 否则是不能获取详细信息的, 例如玩家信息

### 代理(可选)
DstDetailsProxyUrl 是一个批量请求详细数据的代理服务器的Url  
它相当于`https://lobby-v2-cdn.klei.com`的代理, 可以自己搭建代理服务器  
`body`将会传入`[{"RowId":"KU_XXXXXXXX","Region":"v2-ap-east-1"}]`这样的的列表, 并返回`{"GET":[xxx]}`  
不使用代理则请求非常慢

## 接口

https://dstserverlist.top/api

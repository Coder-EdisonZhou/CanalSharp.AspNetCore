# CanalSharp.AspNetCore
一个基于CanalSharp（一款针对.NET的Canal客户端开源项目）封装的ASP.NET Core业务组件，可以用于实时收集MySql数据更改记录并写入修改日志数据表中，可以选择MySql或者MongoDB作为输出记录。

# 关于CanalSharp
CanalSharp 是阿里巴巴开源项目 Canal 的 .NET 客户端。为 .NET 开发者提供一个更友好的使用 Canal 的方式。Canal 是mysql数据库binlog的增量订阅&消费组件，其作者是[WithLin](https://github.com/WithLin)和[晓晨](https://github.com/stulzq)。<br/>
更多关于CanalSharp的信息请浏览：https://github.com/CanalClient/CanalSharp<br/>
更多关于Canal的信息请浏览：https://github.com/alibaba/canal

# 关于此组件
CanalSharp.AspNetCore是一个基于CanalSharp的适用于ASP.NET Core的一个后台任务组件，它可以随着ASP.NET Core实例的启动而启动，目前采用轮询的方式对Canal Server进行监听，获得MySql行更改（RowChange）后写入MySql或MongoDB中指定的记录表中。

# 准备工作
当前的canal开源版本支持8.0及以下的版本，针对阿里云RDS账号默认已经有binlog dump权限，不需要任何权限或者binlog设置，可以直接跳过这一步。
开启binlog写入功能，并且配置binlog模式为row。<br/>
修改C:\ProgramData\MySQL\MySQL Server 5.7\my.ini的以下内容
```sh
log-bin=mysql-bin
binlog-format=Row
server-id=1
```


重启数据库服务，测试修改是否生效
```sh
show variables like 'binlog_format';
show variables like 'log_bin';
```

创建一个Canal用于获取binlog的用户并授予权限
```sh
CREATE USER canal IDENTIFIED BY canal; 
GRANT SELECT, REPLICATION SLAVE, REPLICATION CLIENT ON *.* TO canal @'%';
FLUSH PRIVILEGES;
```

# 安装Canal-Server
通过Docker拉取Canal镜像：
```sh
docker pull canal/canal-server:v1.1.2
```
通过以下命令启动Canal实例：
```sh
docker run --restart=always --name core_productservice_canal \
-e canal.instance.master.address=192.168.16.150:3306 \
-e canal.instance.dbUsername=canal \
-e canal.instance.dbPassword=canal \
-e canal.destinations=products \
-e canal.instance.defaultDatabaseName=products_dev \
-e canal.instance.filter.regex=products_dev\\..* \
-e canal.instance.filter.black.regex=products_dev\\.canal.* \
-p 8001:11111 \
-d canal/canal-server:v1.1.2
```
`PS`: 其中name、destinations、defaultDatabaseName、filter根据要监听的业务数据库按需修改。

# 使用CanalSharp.AspNetCore
首先，通过NuGet或项目引用添加该组件，搜索CanalSharp.AspNetCore：[![CanalSharp.AspNetCore](https://img.shields.io/nuget/v/CanalSharp.AspNetCore.svg)](https://www.nuget.org/packages/CanalSharp.AspNetCore/) | [![CanalSharp.AspNetCore](https://img.shields.io/nuget/dt/CanalSharp.AspNetCore.svg)](https://www.nuget.org/packages/CanalSharp.AspNetCore/)
[![N|Nuget](https://www.cnblogs.com/images/cnblogs_com/edisonchou/1260867/o_Nuget.png)](https://www.cnblogs.com/images/cnblogs_com/edisonchou/1260867/o_Nuget.png)<br/>

其次，在配置文件（appSettings.json)中添加以下配置项：MySql和MongoDB是二选一的配置，不可同时设置。
```sh
"Canal": {
    "Enabled": true,
    "LogSource": "Core.Product.Canal",
    "ServerIP": "192.168.16.190", // Canal-Server所在的服务器IP
    "ServerPort": 8001, // Canal-Server所在的服务器Port
    "Destination": "products", // 建议与Canal-Server中配置的destination保持一致
    "Filter": "products_dev\\..*", // 建议与Canal-Server中配置的filter保持一致
    "SleepTime": 50, // SleepTime越短监听频率越高但也越耗CPU
    "BufferSize": 2048, // 每次监听获取的数据量大小，单位为字节
    "Output": {
      "MySql":{
        "ConnStr": "Server=192.168.16.150;Port=3306;Database=products_dev;Uid=dev;Pwd=xdp" // 要输出的日志记录表所在的MySql连接字符串
      },
      "Mongo":{
        "ConnStr": "mongodb://192.168.16.150:27017", // 要输出的日志记录表所在的MongDB连接字符串
        "DataBase": "productrs_dev" // 要输出的日志记录表所在的MongDB数据库名
      },
    }
  }
```
最后，在StartUp类中的Configure方法中加入以下代码行：
```sh
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    ......
    app.UseCanalClient(Configuration);
}
```

# 示例项目
>[CanalSharp.AspNetCore.Sample](https://github.com/EdisonChou/CanalSharp.AspNetCore/tree/master/sample)

# 效果展示
当在指定要监听的数据库对某张表的某行数据进行Update或Delete操作后，又或者进行Insert行操作后。
## MySql
如果选择输出到MySql数据库，那么canal.logs表会自动记录变更的记录数据如下图：
[![N|DEMO1](https://www.cnblogs.com/images/cnblogs_com/edisonchou/1260867/o_canal.logs.show.png)](https://www.cnblogs.com/images/cnblogs_com/edisonchou/1260867/o_canal.logs.show.png)<br/>
`PS`: INSERT操作会记录新增的数据行数据到CurrentValue列，DELETE操作会记录删除的数据行数据到PreviousValue列，UPDATE操作则会记录修改前PreviousValue和修改后的值CurrentValue。
## MongoDB
如果选择输出到MongoDB，那么会自动记录变更数据如下图：
[![N|DEMO2](https://www.cnblogs.com/images/cnblogs_com/edisonchou/1260867/o_MongoDB_Record.png)](https://www.cnblogs.com/images/cnblogs_com/edisonchou/1260867/o_MongoDB_Record.png)





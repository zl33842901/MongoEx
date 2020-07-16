# MongoEx

针对 MongoDB 的仓储模式ORM

这个包是从网上复制过来的代码，并非原创。

### 安装程序包：

dotnet add package xLiAd.MongoEx.Repository

### 使用方法：

```csharp
//定义实体类
public class UserModel : EntityModel
{
    public string Name { get; set; }
}
var userRepository => new MongoRepository<UserModel>(mongoUrl);//mongoUrl: "mongodb://127.0.0.1:27017/MyDatabase"
await userRepository.AddAsync(new UserModel() { Name = "小新" });
```

另外，我还写了一个基于本仓储的记录操作日志的项目，可以在需要“事件溯源”的时候使用。xLiAd.MongoEx.VersionRepository 具体参见单元测试。
# MongoEx

��� MongoDB �Ĳִ�ģʽORM

������Ǵ����ϸ��ƹ����Ĵ��룬����ԭ����

### ��װ�������

dotnet add package xLiAd.MongoEx.Repository

### ʹ�÷�����

```csharp
//����ʵ����
public class UserModel : EntityModel
{
    public string Name { get; set; }
}
var userRepository => new MongoRepository<UserModel>(mongoUrl);//mongoUrl: "mongodb://127.0.0.1:27017/MyDatabase"
await userRepository.AddAsync(new UserModel() { Name = "С��" });
```

���⣬�һ�д��һ�����ڱ��ִ��ļ�¼������־����Ŀ����������Ҫ���¼���Դ����ʱ��ʹ�á�xLiAd.MongoEx.VersionRepository ����μ���Ԫ���ԡ�
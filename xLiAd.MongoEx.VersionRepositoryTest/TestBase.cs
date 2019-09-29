using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.MongoEx.VersionRepository;

namespace xLiAd.MongoEx.VersionRepositoryTest
{
    public abstract class TestBase
    {
        protected MongoUrl mongoUrl => new MongoUrl("mongodb://zhanglei20:password@172.16.8.77:27017/VersionTest?authSource=admin&authMechanism=SCRAM-SHA-1");
        protected VersionMongoRepository<UserModel> userRepository => new VersionMongoRepository<UserModel>(mongoUrl);

        protected void ClearDatabase()
        {
            MongoClient mc = new MongoClient(mongoUrl);
            mc.DropDatabase("VersionTest");
        }
    }
}

using MongoDB.Driver;
using System;
using xLiAd.MongoEx.Repository;
using xLiAd.MongoEx.VersionRepository;
using Xunit;

namespace xLiAd.MongoEx.VersionRepositoryTest
{
    public class UnitTest1
    {
        private MongoUrl mongoUrl => new MongoUrl("mongodb://zhanglei20:password@172.16.8.77:27017/VersionTest?authSource=admin&authMechanism=SCRAM-SHA-1");
        private VersionMongoRepository<UserModel> userRepository => new VersionMongoRepository<UserModel>(mongoUrl);
        [Fact]
        public void Test1()
        {
            var model = new UserModel()
            {
                CName = "ÕÅÀÚ",
                EmployeeCode = "20720",
                Mail = "zhanglei20@cig.com"
            };
            userRepository.AddOrEdit(model, new DateTime(2019, 8, 20, 15, 30, 20));
            model.Mail = "itc@cig.com";
            model.CName = "¹þ¹þ";
            userRepository.AddOrEdit(model, new DateTime(2019, 9, 20, 15, 30, 20));
            var m1 = userRepository.GetModel("20720", new DateTime(2019, 8, 21));
            var m2 = userRepository.GetModel("20720", DateTime.Now);
            Assert.NotEqual(m1.CName, m2.CName);
        }
    }
}

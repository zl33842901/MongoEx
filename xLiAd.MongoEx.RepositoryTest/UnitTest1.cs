using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using xLiAd.MongoEx.Repository;
using Xunit;

namespace xLiAd.MongoEx.RepositoryTest
{
    public class UnitTest1
    {
        private MongoUrl mongoUrl => new MongoUrl("mongodb://172.16.8.77:27017/ELog");
        private IRepository<UserModel> userRepository => new MongoRepository<UserModel>(mongoUrl);
        [Fact]
        public async Task Test1()
        {
            await userRepository.AddAsync(new UserModel() { Name = "¹þ¹þ¹þ" });
        }
        [Fact]
        public async Task TestQuery()
        {
            var result = await userRepository.AllAsync();
        }
    }
}

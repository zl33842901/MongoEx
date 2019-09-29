using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.MongoEx.VersionRepository;
using Xunit;

namespace xLiAd.MongoEx.VersionRepositoryTest
{
    public class AddTest : TestBase
    {
        [Fact]
        public void Test1()
        {
            var time1 = new DateTime(2019, 8, 20, 15, 30, 20);
            var model = new UserModel()
            {
                CName = "张磊",
                EmployeeCode = "20720",
                Mail = "zhanglei20@cig.com"
            };
            userRepository.Add(model, time1);

            var result = userRepository.GetModel("20720", time1);
            Assert.NotNull(result);
            result = userRepository.GetModel("20720", null);
            Assert.NotNull(result);
            result = userRepository.GetModel("20720", time1.AddMonths(1));
            Assert.Null(result);
            ClearDatabase();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace xLiAd.MongoEx.VersionRepositoryTest
{
    public class EditTest : TestBase
    {
        [Fact]
        public void Test1()
        {
            var model = new UserModel()
            {
                CName = "张磊",
                EmployeeCode = "20720",
                Mail = "zhanglei20@cig.com"
            };
            userRepository.AddOrEdit(model, new DateTime(2019, 8, 20, 15, 30, 20));
            model.Mail = "itc@cig.com";
            model.CName = "哈哈";
            userRepository.AddOrEdit(model, new DateTime(2019, 9, 20, 15, 30, 20));
            var m0 = userRepository.GetModel("20720", new DateTime(2019, 8, 20));
            var m1 = userRepository.GetModel("20720", new DateTime(2019, 8, 21));
            var m2 = userRepository.GetModel("20720", new DateTime(2019, 9, 20));
            var m3 = userRepository.GetModel("20720", new DateTime(2019, 9, 21));
            var m4 = userRepository.GetModel("20720", DateTime.Now);
            Assert.Null(m0);
            Assert.NotNull(m1);
            Assert.Equal(m1.CName, m2.CName);
            Assert.Equal(m1.Mail, m2.Mail);
            Assert.NotEqual(m3.CName, m2.CName);
            Assert.NotEqual(m3.Mail, m2.Mail);
            Assert.Equal(m3.CName, m4.CName);
            Assert.Equal(m3.Mail, m4.Mail);
            ClearDatabase();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using xLiAd.MongoEx.VersionRepository;

namespace xLiAd.MongoEx.VersionRepositoryTest
{
    public class UserModel : VersionEntityModel
    {
        [Key]
        public string EmployeeCode { get; set; }
        public string CName { get; set; }
        public string Mail { get; set; }
        public DateTime? BirthDay { get; set; }
    }
}

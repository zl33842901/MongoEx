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
        public int Class { get; set; }
    }

    public class CompareTestModel : VersionEntityModel
    {
        public int propertyInt { get; set; }
        public int? propertyNullableInt { get; set; }
        public string propertyString { get; set; }
        public float propertyFloat { get; set; }
        public float? propertyNullableFloat { get; set; }
        public DateTime propertyDateTime { get; set; }
        public DateTime? propertyNullableDateTime { get; set; }
        public bool propertyBool { get; set; }
        public bool? propertyNullableBool { get; set; }
        public object propertyObject { get; set; }
        public UserModel propertyClass { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.MongoEx.VersionRepository;
using Xunit;

namespace xLiAd.MongoEx.VersionRepositoryTest
{
    public class ModelCompareTest
    {
        private (CompareTestModel, CompareTestModel) GetSameModel()
        {
            var model = new CompareTestModel()
            {
                propertyBool = false,
                propertyClass = new UserModel(),
                propertyDateTime = DateTime.Today,
                propertyFloat = 1f,
                propertyInt = 4,
                propertyNullableBool = null,
                propertyNullableDateTime = null,
                propertyNullableFloat = 2f,
                propertyNullableInt = 3,
                propertyObject = new object(),
                propertyString = "a"
            };
            var model2 = new CompareTestModel()
            {
                propertyBool = false,
                propertyClass = new UserModel(),
                propertyDateTime = DateTime.Today,
                propertyFloat = 1f,
                propertyInt = 4,
                propertyNullableBool = null,
                propertyNullableDateTime = null,
                propertyNullableFloat = 2f,
                propertyNullableInt = 3,
                propertyObject = new object(),
                propertyString = "a"
            };
            return (model, model2);
        }

        private (CompareTestModel, CompareTestModel) GetDiffModel()
        {
            var model = new CompareTestModel()
            {
                propertyBool = false,
                propertyClass = new UserModel(),
                propertyDateTime = DateTime.Today,
                propertyFloat = 1f,
                propertyInt = 4,
                propertyNullableBool = null,
                propertyNullableDateTime = null,
                propertyNullableFloat = 2f,
                propertyNullableInt = 3,
                propertyObject = new object(),
                propertyString = "a"
            };
            var model2 = new CompareTestModel()
            {
                propertyBool = true,
                propertyClass = null,
                propertyDateTime = DateTime.MinValue,
                propertyFloat = 2f,
                propertyInt = 56,
                propertyNullableBool = true,
                propertyNullableDateTime = new DateTime(),
                propertyNullableFloat = null,
                propertyNullableInt = null,
                propertyObject = null,
                propertyString = "b"
            };
            return (model, model2);
        }
        [Fact]
        public void Test1()
        {
            (var model, var model2) = GetSameModel();
            var result = xLiAd.MongoEx.VersionRepository.ModelCompareHelper.Compare(model, model2, new string[] { "ChangeRecords", "CreatedOn", "Id", "ModifiedOn", "ObjectId" });
            Assert.Empty(result);
        }
        [Fact]
        public void Test2()
        {
            var (model, model2) = GetDiffModel();
            var result = xLiAd.MongoEx.VersionRepository.ModelCompareHelper.Compare(model, model2, new string[] { "ChangeRecords", "CreatedOn", "Id", "ModifiedOn", "ObjectId" });
            Assert.Equal(11, result.Count);
        }

        [Fact]
        public void Test3()
        {
            var (model, model2) = GetDiffModel();
            var result = xLiAd.MongoEx.VersionRepository.ModelCompareHelper.Compare(model, model2, typeof(IVersionEntityModel));
            Assert.Equal(11, result.Count);
        }
    }
}

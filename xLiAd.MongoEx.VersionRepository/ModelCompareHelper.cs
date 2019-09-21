using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository
{
    internal static class ModelCompareHelper
    {
        public static List<FieldChangeRecordDto> Compare<T>(T modelNew, T modelOld, DateTime changeTime) where T : IVersionEntityModel
        {
            if (modelNew == null || modelOld == null)
                throw new Exception("Entity Can't be null.");
            List<FieldChangeRecordDto> result = new List<FieldChangeRecordDto>();
            var properties = TypeHelper.GetProperties(typeof(T));
            foreach(var property in properties)
            {
                var newValue = property.GetValue(modelNew);
                var oldValue = property.GetValue(modelOld);
                var equalsMethod = property.PropertyType.GetMethod("Equals", new Type[] { property.PropertyType, property.PropertyType });
                bool ifeq = (bool)equalsMethod.Invoke(null, new object[] { newValue, oldValue });
                if (!ifeq)
                    result.Add(new FieldChangeRecordDto()
                    {
                        FieldName = property.Name,
                        HappenTime = DateTime.Now,
                        NewValue = newValue,
                        OldValue = oldValue,
                        propertyInfo = property,
                        RecordTime = changeTime
                    });
            }
            return result;
        }
    }
}

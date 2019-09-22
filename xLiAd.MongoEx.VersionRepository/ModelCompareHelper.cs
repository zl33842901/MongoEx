using System;
using System.Collections.Generic;
using System.Reflection;
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
                bool ifeq = IfEquals(property, newValue, oldValue);
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

        private static bool IfEquals(PropertyInfo property, object t1, object t2)
        {
            if (property.PropertyType == typeof(bool))
            {
                return ((bool)t1) == ((bool)t2);
            }
            var equalsMethod = property.PropertyType.GetMethod("Equals", new Type[] { property.PropertyType, property.PropertyType });
            if(equalsMethod != null)
                return (bool)equalsMethod.Invoke(null, new object[] { t1, t2 });
            if(IsNullableType(property.PropertyType))
            {
                if (t1 == null && t2 == null)
                    return true;
                else if (t1 == null || t2 == null)
                    return false;
                else
                {
                    var type = GetNonNullableType(property.PropertyType);
                    equalsMethod = type.GetMethod("Equals", new Type[] { type, type });
                    if (equalsMethod != null)
                        return (bool)equalsMethod.Invoke(null, new object[] { t1, t2 });
                    else
                        throw new Exception("Can't Find Compare Method.");
                }
            }
            throw new Exception("Can't Find Compare Method.");
        }

        private static bool IsNullableType(Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static Type GetNonNullableType(Type type)
        {
            if (IsNullableType(type))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }
    }
}

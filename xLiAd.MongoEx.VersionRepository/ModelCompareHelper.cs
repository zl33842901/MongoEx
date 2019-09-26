using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository
{
    public static class ModelCompareHelper
    {
        public static List<FieldChangeRecordDto> Compare<T>(T modelNew, T modelOld, DateTime changeTime) where T : class,IVersionEntityModel
        {
            var result = Compare(modelNew, modelOld);
            result.ForEach(x => x.RecordTime = changeTime);
            return result;
        }
        public static List<FieldChangeRecordDto> Compare<T>(T modelNew, T modelOld, string[] IgnoreProperties = null) where T : class
        {
            List<FieldChangeRecordDto> result = new List<FieldChangeRecordDto>();
            if (modelNew == null || modelOld == null)
                return result;
            var properties = TypeHelper.GetProperties(typeof(T));
            foreach(var property in properties)
            {
                if (IgnoreProperties != null && IgnoreProperties.Contains(property.Name))
                    continue;
                var newValue = property.GetValue(modelNew);
                var oldValue = property.GetValue(modelOld);
                bool ifeq = IfEquals(property.PropertyType, newValue, oldValue);
                if (!ifeq)
                    result.Add(new FieldChangeRecordDto()
                    {
                        FieldName = property.Name,
                        HappenTime = DateTime.Now,
                        NewValue = newValue,
                        OldValue = oldValue,
                        propertyInfo = property
                    });
            }
            return result;
        }

        public static bool IfEquals(Type type, object t1, object t2)
        {
            MethodInfo equalsMethod;
            if (type.IsValueType)
            {
                if (IsNullableType(type))
                {
                    if (CanConfirmNullEqual(t1, t2, out var b))
                        return b;
                    var acttype = GetNonNullableType(type);
                    equalsMethod = acttype.GetStaticEquals();
                    if (equalsMethod != null)
                        return (bool)equalsMethod.Invoke(null, new object[] { t1, t2 });
                    equalsMethod = acttype.GetInstanceEquals();
                    if (equalsMethod != null)
                        return (bool)equalsMethod.Invoke(t1, new object[] { t2 });
                    else
                        throw new Exception("Can't Find Compare Method.");
                }
                equalsMethod = type.GetInstanceEquals();
                if (equalsMethod != null)
                    return (bool)equalsMethod.Invoke(t1, new object[] { t2 });
            }
            else
            {
                if (CanConfirmNullEqual(t1, t2, out var b))
                    return b;
                if (type == typeof(object))
                    return true;
                equalsMethod = type.GetStaticEquals();
                if (equalsMethod != null)
                    return (bool)equalsMethod.Invoke(null, new object[] { t1, t2 });
                equalsMethod = type.GetInstanceEquals();
                if (equalsMethod != null)
                    return (bool)equalsMethod.Invoke(t1, new object[] { t2 });
                equalsMethod = type.GetMethod("Equals", new Type[] { typeof(object), typeof(object) });
                if (equalsMethod != null)
                    return (bool)equalsMethod.Invoke(null, new object[] { t1, t2 });
                var diff = Compare(t1, t2);
                return diff.Count < 1;
            }
            throw new Exception("Can't Find Compare Method.");
        }
        private static bool CanConfirmNullEqual(object o1, object o2, out bool result)
        {
            if (o1 == null && o2 == null)
            {
                result = true;
                return true;
            }
            else if (o1 == null || o2 == null)
            {
                result = false;
                return true;
            }
            result = false;
            return false;
        }
        private static MethodInfo GetInstanceEquals(this Type type)
        {
            var equalsMethod = type.GetMethod("Equals", new Type[] { type });
            if (equalsMethod == null)
                return null;
            if (equalsMethod.DeclaringType != type)
                return null;
            if (equalsMethod.IsStatic)
                return null;
            else
                return equalsMethod;
        }
        private static MethodInfo GetStaticEquals(this Type type)
        {
            var equalsMethod = type.GetMethod("Equals", new Type[] { type, type });
            if (equalsMethod == null)
                return null;
            if (equalsMethod.DeclaringType != type)
                return null;
            if (equalsMethod.IsStatic)
                return equalsMethod;
            else
                return null;
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

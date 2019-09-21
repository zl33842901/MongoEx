using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository
{
    internal static class TypeHelper
    {
        /// <summary>
        /// 获取主键 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static PropertyInfo GetKeyPropertity(this Type type)
        {
            var properties = type.GetProperties().Where(a => a.GetCustomAttribute<KeyAttribute>() != null).ToArray();

            if (properties.Length > 1)
                throw new Exception($"the {nameof(type)} entity with greater than one KeyAttribute Propertity");
            if (!properties.Any())
            {
                properties = type.GetProperties().Where(a => a.Name == "Id").ToArray();
                if (!properties.Any())
                    throw new Exception($"the {nameof(type)} entity with no KeyAttribute Propertity");
            }

            return properties.First();
        }

        private static ConcurrentDictionary<Type, PropertyInfo[]> ValidProperties = new ConcurrentDictionary<Type, PropertyInfo[]>();
        internal static PropertyInfo[] GetProperties(this Type type)
        {
            if (ValidProperties.ContainsKey(type))
                return ValidProperties[type];
            else
            {
                var result = GetPropertiesValid(type);
                ValidProperties.TryAdd(type, result);
                return result;
            }
        }
        private static PropertyInfo[] GetPropertiesValid(this Type type)
        {
            var plist = type.GetProperties().Where(x => x.CanRead && x.CanWrite //可读可写
                && x.SetMethod.IsPublic && x.GetMethod.IsPublic //且读写方法对外
                && !Attribute.IsDefined(x, typeof(DatabaseGeneratedAttribute)) //没有标记为计算值、标识位
                && !Attribute.IsDefined(x, typeof(NotMappedAttribute)) //没有标记为不对应字段
                && !Attribute.IsDefined(x, typeof(MongoDB.Bson.Serialization.Attributes.BsonIgnoreAttribute))
                && x.Name != "Id" && x.Name != "CreatedOn" && x.Name != "ChangeRecords" && x.Name != "ModifiedOn" && x.Name != "ObjectId"
                ).ToArray();
            return plist;
        }
    }
}

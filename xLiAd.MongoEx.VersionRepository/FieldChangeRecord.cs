using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository
{
    public class FieldChangeRecord
    {
        public DateTime HappenTime { get; set; }
        public DateTime RecordTime { get; set; }
        public string FieldName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public void Invoke<T>(T model)
        {
            var properties = TypeHelper.GetProperties(typeof(T));
            var property = properties.Where(x => x.Name == FieldName).FirstOrDefault();
            if (property == null)
                throw new Exception("type error");
            property.SetValue(model, this.NewValue);
        }
    }
    public class FieldChangeRecordDto : FieldChangeRecord
    {
        public PropertyInfo propertyInfo { get; set; }
        public FieldChangeRecord ToRecord()
        {
            return new FieldChangeRecord()
            {
                FieldName = this.FieldName,
                HappenTime = this.HappenTime,
                NewValue = this.NewValue,
                OldValue = this.OldValue,
                RecordTime = this.RecordTime
            };
        }
    }
}

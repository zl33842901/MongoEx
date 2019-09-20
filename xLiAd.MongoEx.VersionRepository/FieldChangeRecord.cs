using System;
using System.Collections.Generic;
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
    }
}

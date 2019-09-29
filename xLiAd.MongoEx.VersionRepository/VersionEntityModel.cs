using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.MongoEx.Entities;

namespace xLiAd.MongoEx.VersionRepository
{
    public class VersionEntityModel : EntityModel, IVersionEntityModel
    {
        public List<FieldChangeRecord> ChangeRecords { get; set; } = new List<FieldChangeRecord>();
        public bool Deleted { get; set; }
        public DateTime? DeletedTime { get; set; }
    }

    public interface IVersionEntityModel : IEntityModel
    {
        List<FieldChangeRecord> ChangeRecords { get; set; }
        bool Deleted { get; }
        DateTime? DeletedTime { get; }
        new DateTime CreatedOn { get; set; }
        new DateTime ModifiedOn { get; set; }
    }
}

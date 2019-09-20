using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.MongoEx.Entities;

namespace xLiAd.MongoEx.VersionRepository
{
    public class VersionEntityModel : EntityModel, IVersionEntityModel
    {
        public List<FieldChangeRecord> ChangeRecords { get; set; } = new List<FieldChangeRecord>();
    }

    public interface IVersionEntityModel : IEntityModel
    {
        List<FieldChangeRecord> ChangeRecords { get; set; }
    }
}

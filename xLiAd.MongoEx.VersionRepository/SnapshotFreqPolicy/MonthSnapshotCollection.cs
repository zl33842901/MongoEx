using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository.SnapshotFreqPolicy
{
    public class MonthSnapshotCollection : ISnapshotCollection
    {
        internal MonthSnapshotCollection(string collectionName, DateTime dateTime)
        {
            this.StartDate = dateTime;
            this.CollectionName = collectionName;
        }
        public DateTime StartDate { get; private set; }
        public string CollectionName { get; private set; }
        public int Year => StartDate.Year;
        public int Month => StartDate.Month;
        public string SnapshotName => $"{CollectionName}_{StartDate.ToString("yyyy_MM")}";
    }
}

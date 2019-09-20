using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository.SnapshotFreqPolicy
{
    public class MonthSnapshot : ISnapshotFreqPolicy
    {
        public string GetSnapshotCollectionName(string lastestCollectionName, DateTime documentTime)
        {
            var result = $"{lastestCollectionName}_{documentTime.ToString("yyyy_MM")}";
            return result;
        }
    }
}

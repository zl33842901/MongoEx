using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository
{
    public interface ISnapshotFreqPolicy
    {
        string GetSnapshotCollectionName(string lastestCollectionName, DateTime documentTime);
        IEnumerable<string> GetSnapshotCollectionNamesUntilNow(string lastestCollectionName, DateTime documentTime);
    }
}

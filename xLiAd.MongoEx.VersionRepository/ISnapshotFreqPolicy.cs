using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.MongoEx.VersionRepository.SnapshotFreqPolicy;

namespace xLiAd.MongoEx.VersionRepository
{
    public interface ISnapshotFreqPolicy
    {
        string GetSnapshotCollectionName(string lastestCollectionName, DateTime documentTime);
        IEnumerable<string> GetSnapshotCollectionNamesUntilNow(string lastestCollectionName, DateTime documentTime);
        IEnumerable<ISnapshotCollection> GetVersions(string lastestCollectionName, DateTime documentTime, int count = 240);
    }
}

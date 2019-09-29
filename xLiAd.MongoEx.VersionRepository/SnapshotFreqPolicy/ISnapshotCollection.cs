using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository.SnapshotFreqPolicy
{
    public interface ISnapshotCollection
    {
        string SnapshotName { get; }
        DateTime StartDate { get; }
    }
}

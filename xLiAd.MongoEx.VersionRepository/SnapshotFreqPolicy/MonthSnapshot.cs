using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.MongoEx.VersionRepository.SnapshotFreqPolicy
{
    public class MonthSnapshot : ISnapshotFreqPolicy
    {
        /// <summary>
        /// 返回某个时间点的快照表名
        /// </summary>
        /// <param name="lastestCollectionName"></param>
        /// <param name="documentTime"></param>
        /// <returns></returns>
        public string GetSnapshotCollectionName(string lastestCollectionName, DateTime documentTime)
        {
            var result = $"{lastestCollectionName}_{documentTime.ToString("yyyy_MM")}";
            return result;
        }

        /// <summary>
        /// 返回某个时间点到现在的所有快照表名
        /// </summary>
        /// <param name="lastestCollectionName"></param>
        /// <param name="documentTime"></param>
        /// <returns></returns>
        public IEnumerable<string> GetSnapshotCollectionNamesUntilNow(string lastestCollectionName, DateTime documentTime)
        {
            var thisMonthFirstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var documentMonthFirstDay = new DateTime(documentTime.Year, documentTime.Month, 1);
            for(var i = documentMonthFirstDay; i <= thisMonthFirstDay; i = i.AddMonths(1))
            {
                yield return $"{lastestCollectionName}_{i.ToString("yyyy_MM")}";
            }
        }

        /// <summary>
        /// 返回某个时间点向前追count个版本的集合
        /// </summary>
        /// <param name="lastestCollectionName"></param>
        /// <param name="time"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<ISnapshotCollection> GetVersions(string lastestCollectionName, DateTime documentTime, int count = 240)
        {
            var documentMonthFirstDay = new DateTime(documentTime.Year, documentTime.Month, 1);
            var startMonthFirstDay = documentMonthFirstDay.AddMonths(count);
            for(var i = documentMonthFirstDay; i >= startMonthFirstDay; i = i.AddMonths(-1))
            {
                yield return new MonthSnapshotCollection(lastestCollectionName, i);
            }
        }
    }
}

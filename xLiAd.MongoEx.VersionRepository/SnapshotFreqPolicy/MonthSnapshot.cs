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
    }
}

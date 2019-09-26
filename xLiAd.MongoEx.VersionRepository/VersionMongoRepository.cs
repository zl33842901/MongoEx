using MongoDB.Driver;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using xLiAd.MongoEx.Repository;

namespace xLiAd.MongoEx.VersionRepository
{
    public class VersionMongoRepository<T> where T : class,IVersionEntityModel
    {
        protected string CollectionName { get; private set; }
        protected IConnect Connect { get; private set; }
        protected IMongoCollection<T> LastestCollection { get; private set; }
        protected ISnapshotFreqPolicy SnapshotFreqPolicy { get; private set; }
        private object CopyLocker = new object();
        public VersionMongoRepository(IConnect connect, string collectionName = null, ISnapshotFreqPolicy snapshotFreqPolicy = null)
        {
            this.Connect = connect;
            if (string.IsNullOrEmpty(collectionName))
            {
                CollectionNameAttribute mongoCollectionName = (CollectionNameAttribute)typeof(T).GetTypeInfo().GetCustomAttribute(typeof(CollectionNameAttribute));
                this.CollectionName = (mongoCollectionName != null ? mongoCollectionName.Name : typeof(T).Name.ToLower());
                mongoCollectionName = null;
            }
            else
            {
                this.CollectionName = collectionName;
            }
            this.LastestCollection = this.Connect.Collection<T>(this.CollectionName);
            this.SnapshotFreqPolicy = snapshotFreqPolicy ?? new SnapshotFreqPolicy.MonthSnapshot();
        }
        public VersionMongoRepository(string connectionString, string databaseName, string collectionName = null, ISnapshotFreqPolicy snapshotFreqPolicy = null) : this(new Connect(connectionString, databaseName), collectionName, snapshotFreqPolicy) { }
        public VersionMongoRepository(MongoUrl url, string collectionName = null, ISnapshotFreqPolicy snapshotFreqPolicy = null) : this(new Connect(url), collectionName, snapshotFreqPolicy) { }

        private IMongoCollection<T> GetSnapCollection(DateTime time)
        {
            var snapCollectionName = SnapshotFreqPolicy.GetSnapshotCollectionName(CollectionName, time);
            var snapCollection = this.Connect.Collection<T>(snapCollectionName);
            return snapCollection;
        }
        public void Add(T model, DateTime? modelTime = null)
        {
            DateTime time = modelTime ?? DateTime.Now;
            LastestCollection.InsertOne(model, null, new CancellationToken());
            var snapCollection = GetSnapCollection(time);
            snapCollection.InsertOne(model, null, new CancellationToken());
        }

        public void Edit(T model, DateTime? modelTime = null)
        {
            DateTime time = modelTime ?? DateTime.Now;
            var filterDef = GetFilterDefinitionOfKey(model);
            var modelInDb = LastestCollection.Find(filterDef).FirstOrDefault();
            if (modelInDb == null)
            {
                throw new Exception("can't find such record");
            }
            else
            {
                Edit(model, modelInDb, time);
            }
        }

        private bool Edit(T model, T modelInDb, DateTime modelTime)
        {
            var snapCollection = GetSnapCollection(modelTime);
            var filterDef = GetFilterDefinitionOfKey(model);
            var modelInSnap = snapCollection.Find(filterDef).FirstOrDefault();
            if(modelInSnap == null)
            {
                if (!CopyToSnap(snapCollection))
                    throw new Exception("Database Error.");//这种情况一般是snap有了，但是没有当前数据。 我暂时也不知道如何处理。
            }
            var listChange = ModelCompareHelper.Compare(model, modelInDb, modelTime);
            //下面更新主表字段 和 快照表的ChangeRecords
            UpdateDefinitionBuilder<T> builder = Builders<T>.Update;
            UpdateDefinition<T> update = null;
            foreach(var change in listChange)
            {
                if (update == null)
                    update = builder.Set(change.FieldName, change.NewValue);
                else
                    update = update.Set(change.FieldName, change.NewValue);
            }
            var result = LastestCollection.UpdateOne(filterDef, update);
            if (result.ModifiedCount < 1)
                throw new Exception("Error Happenned when Update Database");
            builder = Builders<T>.Update;
            update = builder.PushEach("ChangeRecords", listChange.Select(x => x.ToRecord()));
            var snapResult = snapCollection.UpdateOne(filterDef, update);
            return true;
        }

        /// <summary>
        /// 从主表复制到
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool CopyToSnap(IMongoCollection<T> snapCollection)
        {
            if (snapCollection.CountDocuments(x => true) > 0)
                return false;
            lock (CopyLocker)
            {
                if (snapCollection.CountDocuments(x => true) > 0)
                    return true;//被别的线程锁定之后再进入的。
                snapCollection.InsertMany(
                    LastestCollection.Find(x => true).ToEnumerable()
                );
            }
            return true;
        }

        #region 获取根据主键找到实体的表达式
        /// <summary>
        /// 获取（找到一个和 model 相同主键的实体）的表达式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private FilterDefinition<T> GetFilterDefinitionOfKey(T model)
        {
            var keyProperty = typeof(T).GetKeyPropertity();
            var filterDef = new FilterDefinitionBuilder<T>().Eq(keyProperty.Name, keyProperty.GetValue(model));
            return filterDef;
        }
        private FilterDefinition<T> GetFilterDefinitionOfKey<TKey>(TKey keyFieldValue)
        {
            var keyProperty = typeof(T).GetKeyPropertity();
            var filterDef = new FilterDefinitionBuilder<T>().Eq(keyProperty.Name, keyFieldValue);
            return filterDef;
        }
        #endregion
        public void AddOrEdit(T model, DateTime? modelTime = null)
        {
            DateTime time = modelTime ?? DateTime.Now;
            var filterDef = GetFilterDefinitionOfKey(model);
            var modelInDb = LastestCollection.Find(filterDef).FirstOrDefault();
            if(modelInDb == null)
            {
                Add(model, time);
            }
            else
            {
                Edit(model, modelInDb, time);
            }
        }

        private T GetModelFromLast<TKey>(TKey key)
        {
            var model = LastestCollection.Find(GetFilterDefinitionOfKey(key)).FirstOrDefault();
            return model;
        }

        public T GetModel<TKey>(TKey key, DateTime? modelTime = null)
        {
            if (modelTime == null)
                return GetModelFromLast(key);
            var snapCollection = GetSnapCollection(modelTime.Value);
            var model = snapCollection.Find(GetFilterDefinitionOfKey(key)).FirstOrDefault();
            var changeList = model.ChangeRecords.Where(x => x.RecordTime < modelTime.Value).OrderBy(x => x.RecordTime);
            foreach(var change in changeList)
            {
                change.Invoke(model);
            }
            return model;
        }
    }
}

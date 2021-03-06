﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using xLiAd.MongoEx.Repository;
using xLiAd.MongoEx.VersionRepository.SnapshotFreqPolicy;

namespace xLiAd.MongoEx.VersionRepository
{
    public class VersionMongoRepository<T> where T : class,IVersionEntityModel
    {
        protected string CollectionName { get; private set; }
        protected IConnect Connect { get; private set; }
        protected IMongoCollection<T> OriginalCollection { get; private set; }
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
            this.OriginalCollection = this.Connect.Collection<T>(this.CollectionName);
            this.SnapshotFreqPolicy = snapshotFreqPolicy ?? new SnapshotFreqPolicy.MonthSnapshot();
        }
        public VersionMongoRepository(string connectionString, string databaseName, string collectionName = null, ISnapshotFreqPolicy snapshotFreqPolicy = null) : this(new Connect(connectionString, databaseName), collectionName, snapshotFreqPolicy) { }
        public VersionMongoRepository(MongoUrl url, string collectionName = null, ISnapshotFreqPolicy snapshotFreqPolicy = null) : this(new Connect(url), collectionName, snapshotFreqPolicy) { }
        /// <summary>
        /// 获取某个时间的快照表
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private IMongoCollection<T> GetSnapCollection(DateTime time)
        {
            var snapCollectionName = SnapshotFreqPolicy.GetSnapshotCollectionName(CollectionName, time);
            var snapCollection = this.Connect.Collection<T>(snapCollectionName);
            return snapCollection;
        }
        private IMongoCollection<T> GetSnapCollection(string snapCollectionName)
        {
            var snapCollection = this.Connect.Collection<T>(snapCollectionName);
            return snapCollection;
        }
        /// <summary>
        /// 获取某个时间以来，有效（有数据）的所有表
        /// </summary>
        /// <param name="time"></param>
        /// <param name="firstValid">第一个（time对应的）强制有效</param>
        /// <returns></returns>
        private IEnumerable<IMongoCollection<T>> GetValidSnapCollectionAfter(DateTime time, bool firstValid = true)
        {
            var snapCollectionNames = SnapshotFreqPolicy.GetSnapshotCollectionNamesUntilNow(CollectionName, time);
            var i = 0;
            foreach(var snapCollectionName in snapCollectionNames)
            {
                var snapCollection = this.Connect.Collection<T>(snapCollectionName);
                if(snapCollection.CountDocuments(x => true) > 0 || (i == 0 && firstValid))
                    yield return snapCollection;
                i++;
            }
        }
        //private IMongoCollection<T> GetNowSnapCollection()
        //{
        //    return GetSnapCollection(DateTime.Now);
        //}
        public void Add(T model, DateTime? modelTime = null)
        {
            DateTime time = modelTime ?? DateTime.Now;
            model.CreatedOn = time;
            OriginalCollection.InsertOne(model, null, new CancellationToken());
            foreach(var snapCollection in GetValidSnapCollectionAfter(time))
            {
                snapCollection.InsertOne(model, null, new CancellationToken());
            }
        }

        public void Edit(T model, DateTime? modelTime = null)
        {
            DateTime time = modelTime ?? DateTime.Now;
            var filterDef = GetFilterDefinitionOfKey(model);
            var modelInDb = OriginalCollection.Find(filterDef).FirstOrDefault();
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
            var filterDef = GetFilterDefinitionOfKey(model);
            var listChange = ModelCompareHelper.Compare(model, modelInDb, modelTime);
            //下面更新主表字段 和 快照表的ChangeRecords
            UpdateDefinitionBuilder<T> builder = Builders<T>.Update;
            UpdateDefinition<T> update = null;

            update = builder.PushEach("ChangeRecords", listChange.Select(x => x.ToRecord()));
            foreach (var snapCollection in GetValidSnapCollectionAfter(modelTime))
            {
                var modelInSnap = snapCollection.Find(filterDef).FirstOrDefault();
                if (modelInSnap == null)
                {
                    if (!CopyToSnap(snapCollection))
                        throw new Exception("Database Error.");//这种情况一般是snap有了，但是没有当前数据。 我暂时也不知道如何处理。
                }
                var snapResult = snapCollection.UpdateOne(filterDef, update);
            }

            update = Builders<T>.Update.Set(x => x.ModifiedOn, modelTime);
            foreach (var change in listChange)
            {
                update = update.Set(change.FieldName, change.NewValue);
            }
            var result = OriginalCollection.UpdateOne(filterDef, update);
            if (result.ModifiedCount < 1)
                throw new Exception("Error Happenned when Update Database");
            
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
                    OriginalCollection.Find(x => !x.Deleted).ToEnumerable()
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
            var modelInDb = OriginalCollection.Find(filterDef).FirstOrDefault();
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
            var model = OriginalCollection.Find(GetFilterDefinitionOfKey(key)).FirstOrDefault();
            return model;
        }

        private IMongoCollection<T> GetLastValidCollection(DateTime documentTime)
        {
            var list = this.SnapshotFreqPolicy.GetVersions(CollectionName, documentTime);
            foreach(var item in list)
            {
                var snapCollection = GetSnapCollection(item.SnapshotName);
                if (snapCollection.CountDocuments(x => true) > 0)
                    return snapCollection;
            }
            return null;
        }

        public T GetModel<TKey>(TKey key, DateTime? modelTime = null)
        {
            if (modelTime == null)
                return GetModelFromLast(key);
            var snapCollection = GetSnapCollection(modelTime.Value);
            var model = snapCollection.Find(GetFilterDefinitionOfKey(key)).ToList().Where(x => !x.Deleted).FirstOrDefault();
            if(model == null)
            {
                //比如8月有数据，9月还没数据，但按9月查数据的情况。
                var lastValidCollection = GetLastValidCollection(modelTime.Value);
                if(lastValidCollection != null)
                {
                    model = snapCollection.Find(GetFilterDefinitionOfKey(key)).ToList().Where(x => !x.Deleted).FirstOrDefault();
                    if(model != null)
                    {
                        foreach(var change in model.ChangeRecords)
                        {
                            change.Invoke(model);
                        }
                        return model;
                    }
                }
                return null;
            }
            var changeList = model.ChangeRecords.Where(x => x.RecordTime < modelTime.Value).OrderBy(x => x.RecordTime);
            foreach(var change in changeList)
            {
                change.Invoke(model);
            }
            if (model.Deleted && model.DeletedTime <= modelTime.Value)
                return null;
            else if (model.CreatedOn > modelTime.Value)
                return null;
            else
                return model;
        }

        public long Delete<TKey>(TKey key, DateTime? modelTime = null)
        {
            var filter = GetFilterDefinitionOfKey(key);
            var result = OriginalCollection.DeleteOne(filter);
            if (result.DeletedCount < 1)
                return 0;
            DateTime time = modelTime ?? DateTime.Now;
            var snapCollection = GetSnapCollection(time);
            var uresult = snapCollection.UpdateOne(filter, Builders<T>.Update.Set(x => x.Deleted, true).Set(x => x.DeletedTime, time));
            return uresult.ModifiedCount;
        }

        public IEnumerable<ISnapshotCollection> GetValidVersions()
        {
            var list = this.SnapshotFreqPolicy.GetVersions(CollectionName, DateTime.Now);
            foreach(var item in list)
            {
                var snapCollection = GetSnapCollection(item.SnapshotName);
                if (snapCollection.CountDocuments(x => true) > 0)
                    yield return item;
            }
        }
        private IMongoCollection<T> GetCollectionSnapOrNon(ISnapshotCollection snapshot)
        {
            IMongoCollection<T> collection;
            if (snapshot == null)
            {
                collection = this.OriginalCollection;
            }
            else
            {
                collection = this.GetSnapCollection(snapshot.SnapshotName);
            }
            return collection;
        }

        public long Count(Expression<Func<T,bool>> filter, ISnapshotCollection snapshot = null)
        {
            var collection = GetCollectionSnapOrNon(snapshot);
            var result = collection.CountDocuments(filter);
            return result;
        }

        public IFindFluent<T,T> Find(Expression<Func<T, bool>> filter, ISnapshotCollection snapshot = null)
        {
            var collection = GetCollectionSnapOrNon(snapshot);
            return collection.Find(filter);
        }
        public IFindFluent<T, T> Find(FilterDefinition<T> filter, ISnapshotCollection snapshot = null)
        {
            var collection = GetCollectionSnapOrNon(snapshot);
            return collection.Find(filter);
        }
        public void Page<TKey>(int pageIndex, int pageSize, Expression<Func<T, TKey>> orderBy, bool isOrderByAsc = true, Expression<Func<T, bool>> where = null, ISnapshotCollection snapshot = null)
        {
            var collection = GetCollectionSnapOrNon(snapshot);
            var repo = new MongoRepository<T>(collection);
            var result = repo.Pagination(pageIndex, pageSize, orderBy, isOrderByAsc, where);
        }
    }
}

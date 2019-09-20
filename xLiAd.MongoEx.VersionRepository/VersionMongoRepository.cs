using MongoDB.Driver;
using System;
using System.Reflection;
using System.Threading;
using xLiAd.MongoEx.Repository;

namespace xLiAd.MongoEx.VersionRepository
{
    public class VersionMongoRepository<T> where T : IVersionEntityModel
    {
        protected string CollectionName { get; private set; }
        protected IConnect Connect { get; private set; }
        protected IMongoCollection<T> LastestCollection { get; private set; }
        protected ISnapshotFreqPolicy SnapshotFreqPolicy { get; private set; }
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
            this.SnapshotFreqPolicy = snapshotFreqPolicy;
        }
        public VersionMongoRepository(string connectionString, string databaseName, string collectionName = null, ISnapshotFreqPolicy snapshotFreqPolicy = null) : this(new Connect(connectionString, databaseName), collectionName, snapshotFreqPolicy) { }
        public VersionMongoRepository(MongoUrl url, string collectionName = null, ISnapshotFreqPolicy snapshotFreqPolicy = null) : this(new Connect(url), collectionName, snapshotFreqPolicy) { }

        public void Add(T model, DateTime? modelTime = null)
        {
            DateTime time = modelTime ?? DateTime.Now;
            LastestCollection.InsertOne(model, null, new CancellationToken());
            var snapCollectionName = SnapshotFreqPolicy.GetSnapshotCollectionName(CollectionName, time);
            var snapCollection = this.Connect.Collection<T>(snapCollectionName);
            snapCollection.InsertOne(model, null, new CancellationToken());
        }

        public void Edit(T model, DateTime? modelTime = null)
        {

        }

        public void ProcessModel(T model, DateTime? modelTime = null)
        {
            

        }
    }
}

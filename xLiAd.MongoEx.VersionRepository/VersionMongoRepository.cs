using MongoDB.Driver;
using System;
using System.Reflection;
using xLiAd.MongoEx.Repository;

namespace xLiAd.MongoEx.VersionRepository
{
    public class VersionMongoRepository<T> where T : IVersionEntityModel
    {
        protected string CollectionName
        {
            get;
            private set;
        }

        protected IConnect Connect
        {
            get;
            private set;
        }
        public VersionMongoRepository(string connectionString, string databaseName, string collectionName)
        {
            this.Connect = new Connect(connectionString, databaseName);
            this.CollectionName = collectionName;
        }
        public VersionMongoRepository(MongoUrl url)
        {
            CollectionNameAttribute mongoCollectionName = (CollectionNameAttribute)typeof(T).GetTypeInfo().GetCustomAttribute(typeof(CollectionNameAttribute));
            this.CollectionName = (mongoCollectionName != null ? mongoCollectionName.Name : typeof(T).Name.ToLower());
            mongoCollectionName = null;
            this.Connect = new Connect(url);
        }

        /// <summary>
        /// Initializes a new instance of the MongoRepository class.
        /// </summary>
        /// <param name="url">Url to use for connecting to MongoDB.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        public VersionMongoRepository(MongoUrl url, string collectionName)
        {
            this.Connect = new Connect(url);
            this.CollectionName = collectionName;
        }

        public void ProcessModel(T model, DateTime? modelTime = null)
        {
            DateTime time = modelTime ?? DateTime.Now;

        }
    }
}

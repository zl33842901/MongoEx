using MongoDB.Driver;
using System;

namespace xLiAd.MongoEx.Repository
{
    public interface IConnect : IDisposable
    {
        MongoDatabaseSettings DatabaseSettings
        {
            get;
        }

        IMongoCollection<T> Collection<T>(string collectionName);
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace xLiAd.MongoEx.Entities
{
    public interface IEntityModel<TKey>
    {
        /// <summary>
        ///     create date
        /// </summary>
        [BsonIgnore]
        DateTime CreatedOn { get; }

        /// <summary>
        ///     id in string format
        /// </summary>
        [BsonId]
        string Id { get; set; }

        /// <summary>
        ///     modify date
        /// </summary>
        DateTime ModifiedOn { get; }

        /// <summary>
        ///     id in objectId format
        /// </summary>
        [BsonIgnore]
        ObjectId ObjectId { get; }
    }

    public interface IEntityModel : IEntityModel<string>
    {
    }
}

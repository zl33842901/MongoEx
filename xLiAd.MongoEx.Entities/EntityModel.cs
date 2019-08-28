using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.MongoEx.Entities
{
    [BsonIgnoreExtraElements(Inherited = true)]
    public class EntityModel<TKey> : IEntityModel<TKey>
    {
        public EntityModel()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedOn = ObjectId.CreationTime;
            ModifiedOn = ObjectId.CreationTime;
        }

        /// <summary>
        ///     create date
        /// </summary>
        /// 
        [BsonElement("_c", Order = 1)]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        ///     id in string format
        /// </summary>
        [JsonProperty(Order = 1)]
        [BsonElement("_Id", Order = 0)]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        /// <summary>
        ///     modify date
        /// </summary>
        [BsonElement("_m", Order = 1)]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        ///     id in objectId format
        /// </summary>
        [JsonIgnore]
        public ObjectId ObjectId
        {
            get
            {
                //Incase, this is required if db record is null
                if (Id == null)
                    Id = ObjectId.GenerateNewId().ToString();
                ObjectId objectId = new ObjectId();
                ObjectId.TryParse(Id, out objectId);
                return objectId;
            }
        }
    }
    public class EntityModel : EntityModel<string>
    {


    }
    public class BsonUtcDateTimeSerializer : DateTimeSerializer
    {
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return new DateTime(base.Deserialize(context, args).Ticks, DateTimeKind.Local);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            var utcValue = new DateTime(value.Ticks, DateTimeKind.Utc);
            base.Serialize(context, args, utcValue);
        }
    }
}

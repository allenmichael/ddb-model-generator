using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.CloudTrail.Model;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReadCWEvents
{
    public class CWEvent<T>
    {
        public string Account { get; set; }

        public string Region { get; set; }

        [JsonProperty("detail-type")]
        public string DetailType { get; set; }

        public string Source { get; set; }

        public DateTimeOffset Time { get; set; }

        public Guid Id { get; set; }

        public ICollection<string> Resources { get; set; }
        public T Detail { get; set; }
    }

    public class CWCustomEvent<T> : Event
    {
        [JsonConverter(typeof(CWCustomEventConverter))]
        public T ResponseElements { get; set; }
    }

    public class CWCustomEventConverter : JsonConverter
    {
        public static readonly string TableDescriptionProperty = "tableDescription";
        public static readonly string TableDescriptionCreationDateProperty = "creationDateTime";
        public static readonly Type[] UsableTypes = new Type[] { typeof(TableDescription) };
        public override bool CanConvert(Type objectType)
        {
            return UsableTypes.Any(t => t == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = JObject.Load(reader);
            if (item.Property(TableDescriptionProperty) != null)
            {
                var table = (JObject)item[TableDescriptionProperty];
                table.Remove(TableDescriptionCreationDateProperty);
                return table.ToObject<TableDescription>();
            }
            else
            {
                throw new Exception("No other types implemented.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
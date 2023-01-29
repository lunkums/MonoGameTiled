using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Tiled.Serialization
{
    public class PropertyJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, Type> CustomTypes = new();
        private readonly Dictionary<string, Type> PrimitiveTypes = new()
        {
            { "bool", typeof(bool) },
            { "color", typeof(Color) },
            { "float", typeof(float) },
            { "file", typeof(string) }, // Path
            { "int", typeof(int) },
            { "object", typeof(TileObject) },
            { "string", typeof(string) }
        };

        public JsonConverter[] Converters { get; set; }
        public override bool CanWrite => false;

        public void RegisterCustomType<T>(string typeName)
        {
            CustomTypes.Add(typeName, typeof(T));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);

            // Add custom serializers
            Array.ForEach(Converters, serializer.Converters.Add);

            string name = null;
            Type type = null;
            Type propertyType = null;
            object value = null;

            if (json.TryGetValue("name", StringComparison.OrdinalIgnoreCase, out JToken nameToken))
            {
                name = nameToken.ToString();
            }

            if (json.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out JToken typeToken))
            {
                type = GetPrimitiveType(typeToken.ToString());
            }

            if (json.TryGetValue("propertytype", StringComparison.OrdinalIgnoreCase, out JToken propertyTypeToken))
            {
                propertyType = GetCustomType(propertyTypeToken.ToString());
            }

            if (json.TryGetValue("value", StringComparison.OrdinalIgnoreCase, out JToken valueBlob))
            {
                if (propertyType != null)
                {
                    value = serializer.Deserialize(valueBlob.CreateReader(), propertyType);
                }
                else if (type != null)
                {
                    value = serializer.Deserialize(valueBlob.CreateReader(), type);
                }
            }

            if (name == null || value == null)
            {
                throw new InvalidOperationException("Invalid property.");
            }

            return new Property()
            {
                Name = name,
                Type = type,
                PropertyType = propertyType,
                Value = value
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Property);
        }

        // Since C# stores primitive types under the System namespace, they need to be converted from Tiled types which
        // don't map nicely using Type.GetType("...")
        private Type GetPrimitiveType(string type)
        {
            return PrimitiveTypes.GetValueOrDefault(type, null);
        }

        private Type GetCustomType(string type)
        {
            return CustomTypes.GetValueOrDefault(type, null);
        }
    }
}

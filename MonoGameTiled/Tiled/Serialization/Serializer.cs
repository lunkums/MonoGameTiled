using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Tiled.Serialization
{
    public class Serializer
    {
        private static JsonConverter[] jsonConverters;
        private static PropertyJsonConverter propertyJsonConverter = new();
        private static TileObjectJsonConverter tileObjectJsonConverter = new();

        public static void RegisterCustomType<T>(string typeName)
        {
            propertyJsonConverter.RegisterCustomType<T>(typeName);
        }

        public static void SetConverters(params JsonConverter[] jsonConverters)
        {
            Serializer.jsonConverters = jsonConverters
                .Concat(new JsonConverter[] { propertyJsonConverter }).ToArray();

            propertyJsonConverter.Converters = Serializer.jsonConverters;
            tileObjectJsonConverter.Converters = Serializer.jsonConverters;

            // Add the tile object converter after setting its local converters to avoid a circular dependency
            Serializer.jsonConverters = jsonConverters
                .Concat(new JsonConverter[] { tileObjectJsonConverter }).ToArray();
        }

        public static T DeserializeFromFilePath<T>(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), jsonConverters);
        }

        public static void CopyFromFilePath<T>(string filePath, T obj) where T : class
        {
            JsonConvert.PopulateObject(File.ReadAllText(filePath), obj,
                new JsonSerializerSettings() { Converters = jsonConverters });
        }
    }
}

using Newtonsoft.Json;
using System.IO;

namespace Tiled.Serialization
{
    public class Serializer
    {
        private static JsonConverter[] jsonConverters;

        public static void SetConverters(params JsonConverter[] jsonConverters)
        {
            Serializer.jsonConverters = jsonConverters;
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

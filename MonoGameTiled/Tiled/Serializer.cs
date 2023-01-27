using Newtonsoft.Json;
using System.IO;

namespace Tiled
{
    public static class Serializer
    {
        public static T DeserializeFromFilePath<T>(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }

        public static void CopyFromFilePath<T>(string filePath, T obj) where T : class
        {
            JsonConvert.PopulateObject(File.ReadAllText(filePath), obj);
        }

        public static T DeserializeFromBlob<T>(string blob)
        {
            return JsonConvert.DeserializeObject<T>(blob);
        }
    }
}

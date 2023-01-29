using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tiled.Serialization
{
    public class TileObjectJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public JsonConverter[] Converters { get; set; }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jObject = (JObject)JToken.ReadFrom(reader);

            // If the object has a template, merge them together, overriding the template's values
            if (jObject.TryGetValue("template", StringComparison.OrdinalIgnoreCase, out JToken templateToken))
            {
                JObject jTemplate
                    = JObject.Parse(File.ReadAllText(Path.Combine(Map.Directory, templateToken.ToString())));
                JObject jTemplateObject
                    = (JObject)jTemplate.GetValue("object", StringComparison.OrdinalIgnoreCase);

                JArray templateProps
                    = (JArray)jTemplateObject.GetValue("properties", StringComparison.OrdinalIgnoreCase);
                JArray objectProps
                    = (JArray)jObject.GetValue("properties", StringComparison.OrdinalIgnoreCase);

                // Merge all of the object's properties into (and overriding) the template's properties
                foreach (JObject prop in templateProps.Cast<JObject>())
                {
                    foreach (JObject prop2 in objectProps.Cast<JObject>())
                    {
                        if ((string)prop.GetValue("name", StringComparison.OrdinalIgnoreCase)
                            == (string)prop2.GetValue("name", StringComparison.OrdinalIgnoreCase))
                        {
                            prop.Merge(prop2, new JsonMergeSettings()
                            {
                                MergeNullValueHandling = MergeNullValueHandling.Ignore,
                                PropertyNameComparison = StringComparison.OrdinalIgnoreCase 
                            });
                        }
                    }
                }

                // Merge the object into its template
                jTemplateObject.Merge(jObject, new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    PropertyNameComparison = StringComparison.OrdinalIgnoreCase,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore
                });
                jObject = jTemplateObject;
            }

            return JsonConvert.DeserializeObject<TileObject>(jObject.ToString(), Converters);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TileObject);
        }
    }
}

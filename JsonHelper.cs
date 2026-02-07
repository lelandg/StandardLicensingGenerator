using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StandardLicensingGenerator
{
    public class JsonHelper
    {
        const string Separator = ":";   // Colon separator for nested paths
        const string RootKey = "$";     // Used when root is array or primitive

        public static IDictionary<string, string> FlattenJsonToDictionary(JToken token)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Flatten(token, path: "", result);
            return result;
        }

        public static void Flatten(JToken token, string path, IDictionary<string, string> output)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var prop in (JObject)token)
                    {
                        var childPath = AppendPath(path, prop.Key);
                        Flatten(prop.Value, childPath, output);
                    }
                    break;

                case JTokenType.Array:
                    var arr = (JArray)token;
                    if (!arr.HasValues)
                    {
                        // record empty arrays explicitly
                        output[string.IsNullOrEmpty(path) ? RootKey : path] = "[]";
                        return;
                    }

                    for (int i = 0; i < arr.Count; i++)
                    {
                        // Use colon for array indices as well (e.g., roles:0, items:2:name)
                        var childPath = AppendPath(path, i.ToString(CultureInfo.InvariantCulture));
                        Flatten(arr[i], childPath, output);
                    }
                    break;

                default:
                    var key = string.IsNullOrEmpty(path) ? RootKey : path;
                    output[key] = ConvertJValueToString(token);
                    break;
            }
        }

        public static string AppendPath(string basePath, string nextSegment)
        {
            if (string.IsNullOrEmpty(basePath))
                return nextSegment;
            return $"{basePath}{Separator}{nextSegment}";
        }

        public static string ConvertJValueToString(JToken token)
        {
            if (token is JValue jv)
            {
                switch (jv.Type)
                {
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        return Convert.ToString(jv.Value, CultureInfo.InvariantCulture) ?? string.Empty;

                    case JTokenType.Boolean:
                        return ((bool)jv.Value).ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

                    case JTokenType.Date:
                        if (jv.Value is DateTime dt)
                            return dt.ToString("o", CultureInfo.InvariantCulture);
                        if (jv.Value is DateTimeOffset dto)
                            return dto.ToString("o", CultureInfo.InvariantCulture);
                        return jv.ToString(CultureInfo.InvariantCulture);

                    case JTokenType.Null:
                        return string.Empty;

                    case JTokenType.String:
                        return jv.Value?.ToString() ?? string.Empty;

                    default:
                        return token.ToString(Formatting.None);
                }
            }

            return token.ToString(Formatting.None);
        }
    }
}

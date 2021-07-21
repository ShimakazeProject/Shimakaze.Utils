using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Utils.Csf.Utils.Json.V1
{
    public static class CsfJsonConverterUtils
    {
        internal static JsonConverter<T>? GetConverter<T>(this JsonSerializerOptions options)
            => options.GetConverter(typeof(T)) as JsonConverter<T>;
        //=> options.Converters.Where(i => i is JsonConverter<T>).Select(i => i as JsonConverter<T>).First();

        internal static T? Read<T>(this JsonConverter<T> @this, ref Utf8JsonReader reader, JsonSerializerOptions options) => @this.Read(ref reader, typeof(T), options);

        private static JsonSerializerOptions? _csfJsonSerializerOptions = null;
        /// <summary>
        /// (Lazy Design)
        /// </summary>
        public static JsonSerializerOptions CsfJsonSerializerOptions
        {
            get
            {
                if (_csfJsonSerializerOptions is JsonSerializerOptions result)
                    return result;
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    //IncludeFields = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    AllowTrailingCommas = true,

                };
                options.Converters.Add(new CsfStructJsonConverter());
                options.Converters.Add(new CsfHeadJsonConverter());
                options.Converters.Add(new CsfLabelsJsonConverter());
                options.Converters.Add(new CsfLabelJsonConverter());
                options.Converters.Add(new CsfValuesJsonConverter());
                options.Converters.Add(new CsfValueJsonConverter());
                options.Converters.Add(new Common.MultiLineStringJsonConverter());
                return _csfJsonSerializerOptions = options;
            }
        }
    }
}

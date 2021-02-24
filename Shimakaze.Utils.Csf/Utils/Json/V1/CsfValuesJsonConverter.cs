using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils.Json.V1
{
    public class CsfValuesJsonConverter : JsonConverter<List<CsfValue>>
    {
        public override List<CsfValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            var converter = options.GetConverter<CsfValue>();
            var result = new List<CsfValue>();
            while (reader.Read())
            {
                if (reader.TokenType is JsonTokenType.EndArray)
                    break;

                result.Add(converter!.Read(ref reader, options)!);
            }
            return result;
        }
        public override void Write(Utf8JsonWriter writer, List<CsfValue> value, JsonSerializerOptions options)
        {
            var converter = options.GetConverter<CsfValue>();
            if (value.Count > 1)
            {
                writer.WritePropertyName(nameof(CsfLabel.Values).ToLower());
                writer.WriteStartArray();
                value.ForEach(i => converter!.Write(writer, i, options));
                writer.WriteEndArray();
            }
            else if (value.Count == 1)
            {
                writer.WritePropertyName("value");
                converter!.Write(writer, value[0], options);
            }
            else{
                Debug.Assert(false);
            }
        }
    }
}

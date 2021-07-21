using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils.Json.V2
{
    public class CsfLabelsJsonConverter : JsonConverter<List<CsfLabel>>
    {
        public override List<CsfLabel> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var converter = options.GetConverter<CsfLabel>();
            var result = new List<CsfLabel>();
            while (reader.Read())
            {
                if (reader.TokenType is JsonTokenType.EndObject)
                    break;

                result.Add(converter!.Read(ref reader, options)!);
            }
            return result;
        }
        public override void Write(Utf8JsonWriter writer, List<CsfLabel> value, JsonSerializerOptions options)
        {
            var converter = options.GetConverter<CsfLabel>();
            writer.WriteStartObject();
            value.ForEach(i => converter!.Write(writer, i, options));
            writer.WriteEndObject();
        }
    }
}

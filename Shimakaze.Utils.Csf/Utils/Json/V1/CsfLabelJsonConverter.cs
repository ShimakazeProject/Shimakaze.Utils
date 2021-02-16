using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils.Json.V1
{
    public class CsfLabelJsonConverter : JsonConverter<CsfLabel>
    {
        public override CsfLabel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var result = new CsfLabel();
            var value = new CsfValue();
            var converter = options.GetConverter<List<CsfValue>>();
            var converter2 = options.GetConverter<string>();
            while (reader.Read())
            {
                if (reader.TokenType is JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();
                switch (reader.GetString()?.ToLower())
                {
                    case "label":
                        reader.Read();
                        result.Label = reader.GetString()!;
                        break;
                    case "values":
                        reader.Read();
                        result.Values = converter!.Read(ref reader, options)!;
                        break;
                    case "value":
                        if (result.Values.Count > 0)
                            throw new JsonException();
                        reader.Read();
                        value.Value = converter2!.Read(ref reader, options)!;
                        break;
                    case "extra":
                        if (result.Values.Count > 0)
                            throw new JsonException();
                        reader.Read();
                        value.Extra = reader.TokenType is JsonTokenType.String ? reader.GetString()! : throw new JsonException();
                        break;
                    default:
                        throw new JsonException();
                }
            }
            if (result.Values.Count < 1)
                result.Values.Add(value);
            return result;
        }
        public override void Write(Utf8JsonWriter writer, CsfLabel value, JsonSerializerOptions options)
        {
            var converter = options.GetConverter<List<CsfValue>>();
            writer.WriteStartObject();
            writer.WriteString(nameof(value.Label).ToLower(), value.Label);
            converter!.Write(writer, value.Values, options);
            writer.WriteEndObject();
        }
    }
}

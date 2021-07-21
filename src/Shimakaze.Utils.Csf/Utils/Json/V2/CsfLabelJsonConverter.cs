using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils.Json.V2
{
    public class CsfLabelJsonConverter : JsonConverter<CsfLabel>
    {
        public override CsfLabel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            var result = new CsfLabel
            {
                Label = reader.GetString()!
            };

            var value = new CsfValue();
            var converter = options.GetConverter<List<CsfValue>>();
            var converter2 = options.GetConverter<string>();

            while (reader.Read())
            {
            OUTER:
                if (reader.TokenType is JsonTokenType.EndObject)
                    break;

                if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.String)
                {
                    value.Value = converter2!.Read(ref reader, options)!;
                    break;
                }
                else if (reader.TokenType is JsonTokenType.StartObject)
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType is JsonTokenType.EndObject)
                            goto OUTER;
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException();
                        if ("values" == reader.GetString()?.ToLower())
                        {
                            reader.Read();
                            result.Values = converter!.Read(ref reader, options)!;
                            break;
                        }
                        switch (reader.GetString()?.ToLower())
                        {
                            case "value":
                                if (result.Values.Count > 0)
                                    throw new JsonException();
                                reader.Read();
                                value.Value = converter2!.Read(ref reader, options)!;
                                continue;
                            case "extra":
                                if (result.Values.Count > 0)
                                    throw new JsonException();
                                reader.Read();
                                value.Extra = reader.TokenType is JsonTokenType.String ? reader.GetString()! : throw new JsonException();
                                continue;
                            default:
                                throw new JsonException();
                        }
                    }
                    continue;
                }
                else
                    throw new JsonException();
            }
            if (result.Values.Count < 1)
                result.Values.Add(value);
            return result;
        }
        public override void Write(Utf8JsonWriter writer, CsfLabel value, JsonSerializerOptions options)
        {
            var converter = options.GetConverter<List<CsfValue>>();
            writer.WritePropertyName(value.Label);
            if (value.Values.Count == 1)
            {
                if (string.IsNullOrEmpty(value.Values[0].Extra))
                {
                    options.GetConverter<CsfValue>()!.Write(writer, value.Values[0], options);
                    return;
                }
            }
            writer.WriteStartObject();
            converter!.Write(writer, value.Values, options);
            writer.WriteEndObject();
        }
    }
}

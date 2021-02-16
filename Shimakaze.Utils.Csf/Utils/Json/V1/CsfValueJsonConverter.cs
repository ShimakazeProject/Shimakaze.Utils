using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils.Json.V1
{
    public class CsfValueJsonConverter : JsonConverter<CsfValue>
    {
        public override CsfValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var csfStr = new CsfValue();
            var converter = options.GetConverter<string>();
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                case JsonTokenType.StartArray:
                    csfStr.Value = converter!.Read(ref reader, options)!;
                    break;
                case JsonTokenType.StartObject:
                    while (reader.Read())
                    {
                        if (reader.TokenType is JsonTokenType.EndObject)
                            break;
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException();
                        switch (reader.GetString()?.ToLower())
                        {
                            case "value":
                                reader.Read();
                                csfStr.Value = converter!.Read(ref reader, options)!;
                                break;
                            case "extra":
                                reader.Read();
                                csfStr.Extra = reader.TokenType is JsonTokenType.String ? reader.GetString()! : throw new JsonException();
                                break;
                            default:
                                throw new JsonException();
                        }
                    }
                    break;
                default:
                    throw new JsonException();
            }
            return csfStr;
        }
        public override void Write(Utf8JsonWriter writer, CsfValue value, JsonSerializerOptions options)
        {
            var converter = options.GetConverter<string>();
            if (string.IsNullOrEmpty(value.Extra))
            {
                converter!.Write(writer, value.Value, options);
            }
            else
            {
                //writer.WriteStartObject();
                //writer.WritePropertyName(nameof(value.Value).ToLower());
                converter!.Write(writer, value.Value, options);

                writer.WriteString(nameof(value.Extra).ToLower(), value.Extra);
                //writer.WriteEndObject();
            }
        }
    }
}

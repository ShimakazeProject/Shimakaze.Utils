using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils.Json.V1
{
    public class CsfStructJsonConverter : JsonConverter<CsfStruct>
    {
        public override CsfStruct Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var result = new CsfStruct();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                switch (reader.GetString()?.ToLower())
                {
                    case "$schema":
                        reader.Skip();
                        break;
                    case "protocol":
                        reader.Read();
                        if (reader.TokenType != JsonTokenType.Number)
                            throw new JsonException();
                        if (reader.GetInt32() != 1)
                            throw new NotSupportedException("Supported protocol Version is 1 but it is " + reader.GetInt32());
                        break;
                    case "head":
                        reader.Read();
                        result.Head = options!.GetConverter<CsfHead>()!.Read(ref reader, options)!;
                        break;
                    case "data":
                        reader.Read();
                        result.Data = options!.GetConverter<List<CsfLabel>>()!.Read(ref reader, options)!;
                        break;
                    default:
                        break;
                }
            }
            result.ReCount();
            return result;
        }
        public override void Write(Utf8JsonWriter writer, CsfStruct value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$schema", SchemaUrls.V1);
            writer.WriteNumber("protocol", 1);
            writer.WritePropertyName(nameof(value.Head).ToLower());
            options!.GetConverter<CsfHead>()!.Write(writer, value.Head, options);
            writer.WritePropertyName(nameof(value.Data).ToLower());
            options!.GetConverter<List<CsfLabel>>()!.Write(writer, value.Data, options);
            writer.WriteEndObject();
        }
    }
}

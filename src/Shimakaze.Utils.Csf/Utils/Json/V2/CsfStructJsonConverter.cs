using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Utils.Csf.Struct;

namespace Shimakaze.Utils.Csf.Utils.Json.V2
{
    public class CsfStructJsonConverter : JsonConverter<CsfStruct>
    {
        public override CsfStruct Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var result = new CsfStruct();
            result.Head.Version = 3;
            result.Head.Language = 0;
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
                        if (reader.GetInt32() != 2)
                            throw new NotSupportedException("Supported protocol Version is 2 but it is " + reader.GetInt32());
                        break;
                    case "version":
                        reader.Read();
                        result.Head.Version = reader.GetInt32();
                        break;
                    case "language":
                        reader.Read();
                        if (reader.TokenType is JsonTokenType.Number)
                        {
                            result.Head.Language = reader.GetInt32();
                        }
                        else if (reader.TokenType is JsonTokenType.String)
                        {
                            var code = reader.GetString();
                            for (result.Head.Language = 0; result.Head.Language < HeadUtil.LanguageList.Length; result.Head.Language++)
                            {
                                if (HeadUtil.LanguageList[result.Head.Language].Equals(code))
                                    break;
                            }
                        }
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
            writer.WriteString("$schema", SchemaUrls.V2);
            writer.WriteNumber("protocol", 2);
            writer.WriteNumber(nameof(value.Head.Version).ToLower(), value.Head.Version);
            writer.WriteNumber(nameof(value.Head.Language).ToLower(), value.Head.Language);

            writer.WritePropertyName(nameof(value.Data).ToLower());
            options!.GetConverter<List<CsfLabel>>()!.Write(writer, value.Data, options);
            writer.WriteEndObject();
        }
    }
}

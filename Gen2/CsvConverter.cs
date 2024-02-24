using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PKHeX.EncounterSlotDumper;

public static class CsvConverter
{
    private const char sep = '\t';

    public static byte[] ConvertCsvToPickle<T>(string text, char separator = sep) where T : IWriteable
    {
        var lines = text.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var arr = ConvertCsvToArray<T>(lines, separator);
        var result = arr.SelectMany(z => z.Write()).ToArray();
        return result;
    }

    public static string ConvertCsvFileToJsonObject(string path, char separator = sep)
    {
        var lines = File.ReadAllLines(path);
        return ConvertToJson(lines, separator);
    }

    public static T[] ConvertCsvToArray<T>(string path, char separator = sep)
    {
        var lines = File.ReadAllLines(path);
        return ConvertCsvToArray<T>(lines, separator);
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        Converters = { new BooleanConverter() },
    };

    public static T[] ConvertCsvToArray<T>(ReadOnlySpan<string> lines, char separator = sep)
    {
        var json = ConvertToJson(lines, separator);
        return JsonSerializer.Deserialize<T[]>(json, Options) ?? throw new Exception("Failed to deserialize JSON.");
    }

    private class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.String:
                    return reader.GetString() switch
                    {
                        "true" => true,
                        "false" => false,
                        _ => throw new JsonException()
                    };
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }

    private static string ConvertToJson(ReadOnlySpan<string> lines, char separator)
    {
        var props = lines[0].Split(separator);

        var result = new List<Dictionary<string, string>>(lines.Length - 1);
        foreach (var line in lines[1..])
        {
            var split = line.Split(separator);
            var obj = new Dictionary<string, string>();
            var length = Math.Min(props.Length, split.Length);
            for (int j = 0; j < length; j++)
            {
                var val = split[j];
                if (string.IsNullOrWhiteSpace(val))
                    continue;
                obj.Add(props[j], val);
            }
            result.Add(obj);
        }
        return JsonSerializer.Serialize(result);
    }
}

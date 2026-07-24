// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using System.Text.Json;
using System.Text.Json.Serialization;
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Foundations.Architectures;

internal static class ArchitectureJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { (JsonConverter)new JsonStringEnumConverter() },
    };

    public static string Serialize(Architecture architecture)
    {
        return JsonSerializer.Serialize(value: architecture, options: SerializerOptions);
    }

    public static Architecture Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Architecture>(json: json, options: SerializerOptions)
            ?? throw new InvalidOperationException(message: "The architecture JSON could not be parsed.");
    }
}
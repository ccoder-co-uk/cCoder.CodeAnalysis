// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Foundations.Architectures;

internal sealed class ArchitectureJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { (JsonConverter)new JsonStringEnumConverter() },
    };

    public static string Serialize(Architecture architecture)
    {
        return JsonSerializer.Serialize(architecture, SerializerOptions);
    }

    public static Architecture Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Architecture>(json, SerializerOptions)
            ?? throw new InvalidOperationException("The architecture JSON could not be parsed.");
    }
}
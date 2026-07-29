// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.BuildTasks;

public sealed class GenerateArchitectureTask : Microsoft.Build.Utilities.Task
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Required]
    public string ProjectPath { get; set; } = string.Empty;

    [Required]
    public string OutputPath { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            ServiceCollection services = new ServiceCollection();
            services.AddCodeAnalysis();

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            IArchitectureBuilder architectureBuilder = serviceProvider.GetRequiredService<IArchitectureBuilder>();
            Architecture architecture = architectureBuilder.Generate(path: ProjectPath);
            string architectureJson = JsonSerializer.Serialize(
                value: architecture,
                options: SerializerOptions);

            WriteArchitectureFile(
                outputPath: OutputPath,
                architectureJson: architectureJson);

            Log.LogMessage(
                importance: MessageImportance.Low,
                message: $"Generated architecture document '{OutputPath}'.");

            return true;
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(
                exception: exception,
                showStackTrace: true);

            return false;
        }
    }

    private static void WriteArchitectureFile(
        string outputPath,
        string architectureJson)
    {
        string? outputDirectory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(path: outputDirectory);
        }

        if (File.Exists(path: outputPath)
            && File.ReadAllText(path: outputPath).Equals(architectureJson, StringComparison.Ordinal))
        {
            return;
        }

        File.WriteAllText(
            path: outputPath,
            contents: architectureJson);
    }
}
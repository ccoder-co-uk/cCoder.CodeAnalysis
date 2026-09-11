// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.CodeAnalysis.BuildTasks;
using FluentAssertions;
using Microsoft.Build.Utilities;

namespace cCoder.CodeAnalysis.Tests.BuildTasks;

public sealed class GenerateArchitectureTaskTests
{
    [Fact]
    public void ReferencePaths_WhenPlatformAssembliesAlreadyExist_AreMerged()
    {
        // Given
        string originalPlatformAssemblies =
            AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES") as string
            ?? string.Empty;
        string referencePath = CreateTemporaryReferencePath();
        GenerateArchitectureTask task = new()
        {
            ReferencePaths = [new TaskItem(referencePath)]
        };
        MethodInfo ensurePlatformAssemblies = typeof(GenerateArchitectureTask)
            .GetMethod(
                name: "EnsurePlatformAssembliesAreAvailable",
                bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic)!;

        try
        {
            // When
            ensurePlatformAssemblies.Invoke(obj: task, parameters: null);

            // Then
            string[] platformAssemblies =
                (AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES") as string
                    ?? string.Empty)
                .Split(separator: Path.PathSeparator);

            platformAssemblies.Should().Contain(referencePath);
            platformAssemblies.Should().Contain(typeof(object).Assembly.Location);
        }
        finally
        {
            AppDomain.CurrentDomain.SetData(
                name: "TRUSTED_PLATFORM_ASSEMBLIES",
                data: originalPlatformAssemblies);
            File.Delete(path: referencePath);
            Directory.Delete(path: Path.GetDirectoryName(path: referencePath)!);
        }
    }

    private static string CreateTemporaryReferencePath()
    {
        string referenceDirectory = Path.Combine(
            path1: Path.GetTempPath(),
            path2: $"CodeAnalysis-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path: referenceDirectory);
        string referencePath = Path.Combine(
            path1: referenceDirectory,
            path2: $"ReferencePath-{Guid.NewGuid():N}.dll");

        File.Copy(
            sourceFileName: typeof(object).Assembly.Location,
            destFileName: referencePath);

        return referencePath;
    }
}

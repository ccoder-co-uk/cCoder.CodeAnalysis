// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Brokers.Files;
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Services.Foundations.Architectures;

internal sealed class ArchitectureService(IFileBroker fileBroker) : IArchitectureService
{
    public ArchitectureBuild Build(string suppliedPath)
    {
        string projectFilePath = ResolveProjectFilePath(path: suppliedPath);

        string projectDirectory =
            Path.GetDirectoryName(path: projectFilePath)
            ?? throw new InvalidOperationException(message: "The project path has no containing directory.");

        SyntaxTree[] projectSyntaxTrees = Directory
            .GetFiles(path: projectDirectory, searchPattern: "*.cs", searchOption: SearchOption.AllDirectories)
            .Where(
                predicate: (string sourcePath) => !IsBuildOutput(path: sourcePath, projectDirectory: projectDirectory)
            )
            .OrderBy(keySelector: (string sourcePath) => sourcePath, comparer: StringComparer.OrdinalIgnoreCase)
            .Select(
                selector: (string sourcePath) =>
                    CSharpSyntaxTree.ParseText(
                        text: File.ReadAllText(path: sourcePath),
                        options: null,
                        path: sourcePath
                    )
            )
            .ToArray();

        SyntaxTree[] compilationSyntaxTrees = [CreateImplicitUsingsSyntaxTree(), .. projectSyntaxTrees];

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: Path.GetFileNameWithoutExtension(path: projectFilePath),
            syntaxTrees: compilationSyntaxTrees,
            references: GetMetadataReferences(projectFilePath: projectFilePath),
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary)
        );

        return CreateArchitectureBuild(compilation: compilation);
    }

    public ArchitectureBuild Build(CSharpCompilation compilation) =>
        CreateArchitectureBuild(compilation: compilation);

    private static ArchitectureBuild CreateArchitectureBuild(CSharpCompilation compilation) =>

        new ArchitectureBuild { Compilation = compilation };

    internal string ResolveProjectFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(value: path))
        {
            throw new ArgumentException(message: "The project path is required.", paramName: nameof(path));
        }

        if (fileBroker.FileExists(path: path))
        {
            if (
                !string.Equals(
                    a: Path.GetExtension(path: path),
                    b: ".csproj",
                    comparisonType: StringComparison.OrdinalIgnoreCase
                )
            )
            {
                throw new ArgumentException(
                    message: "The supplied file is not a .NET project file.",
                    paramName: nameof(path)
                );
            }

            return path;
        }

        if (!fileBroker.DirectoryExists(path: path))
        {
            throw new DirectoryNotFoundException(message: "The project path '" + path + "' does not exist.");
        }

        IReadOnlyList<string> projectFiles = fileBroker.GetProjectFiles(directoryPath: path);

        return projectFiles.Count switch
        {
            0 => throw new FileNotFoundException(
                message: "The supplied directory does not contain a .NET project file.",
                fileName: path
            ),
            1 => projectFiles[index: 0],
            _ => throw new InvalidOperationException(
                message: "The supplied directory contains more than one .NET project file."
            ),
        };
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences(string projectFilePath)
    {
        string trustedAssemblies =
            AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES") as string
            ?? throw new InvalidOperationException(message: "Platform assemblies could not be resolved.");

        IEnumerable<string> platformAssemblies = trustedAssemblies.Split(separator: Path.PathSeparator);
        IEnumerable<string> buildAssemblies = GetBuildAssemblies(projectFilePath: projectFilePath);

        return platformAssemblies
            .Concat(second: buildAssemblies)
            .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
            .Select(selector: (string assemblyPath) => MetadataReference.CreateFromFile(path: assemblyPath));
    }

    private static string[] GetBuildAssemblies(string projectFilePath)
    {
        string projectDirectory =
            Path.GetDirectoryName(path: projectFilePath)
            ?? throw new InvalidOperationException(message: "The project path has no containing directory.");

        string projectName = Path.GetFileNameWithoutExtension(path: projectFilePath);
        string buildDirectory = Path.Combine(path1: projectDirectory, path2: "bin");

        if (!Directory.Exists(path: buildDirectory))
        {
            return [];
        }

        string? projectAssembly = Directory
            .GetFiles(
                path: buildDirectory,
                searchPattern: projectName + ".dll",
                searchOption: SearchOption.AllDirectories
            )
            .OrderByDescending(keySelector: File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        return projectAssembly is null
            ? []
            : Directory.GetFiles(
                path: Path.GetDirectoryName(path: projectAssembly)!,
                searchPattern: "*.dll",
                searchOption: SearchOption.TopDirectoryOnly
            );
    }

    private static SyntaxTree CreateImplicitUsingsSyntaxTree() =>

        CSharpSyntaxTree.ParseText(
            text: "global using System;\r\nglobal using System.Collections.Generic;\r\nglobal using System.IO;\r\nglobal using System.Linq;\r\nglobal using System.Threading;\r\nglobal using System.Threading.Tasks;"
        );

    private static bool IsBuildOutput(string path, string projectDirectory)
    {
        string projectDirectoryPrefix =
            Path.GetFullPath(path: projectDirectory)
            .TrimEnd(trimChars: [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar])
            + Path.DirectorySeparatorChar;

        string fullPath = Path.GetFullPath(path: path);

        string relativePath = fullPath.StartsWith(
            value: projectDirectoryPrefix,
            comparisonType: StringComparison.OrdinalIgnoreCase
        )
            ? fullPath.Substring(startIndex: projectDirectoryPrefix.Length)
            : fullPath;

        return relativePath.StartsWith(
                value: $"bin{Path.DirectorySeparatorChar}",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
            || relativePath.StartsWith(
                value: $"obj{Path.DirectorySeparatorChar}",
                comparisonType: StringComparison.OrdinalIgnoreCase
            );
    }
}

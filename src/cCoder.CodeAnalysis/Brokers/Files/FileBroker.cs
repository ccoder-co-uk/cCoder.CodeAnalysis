// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
namespace cCoder.CodeAnalysis.Brokers.Files;

internal sealed class FileBroker : IFileBroker
{
    public bool FileExists(string path)
    {
        return File.Exists(path: path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path: path);
    }

    public IReadOnlyList<string> GetProjectFiles(string directoryPath) =>

        Directory
            .GetFiles(path: directoryPath, searchPattern: "*.csproj", searchOption: SearchOption.TopDirectoryOnly)
            .OrderBy(keySelector: (string path) => path, comparer: StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
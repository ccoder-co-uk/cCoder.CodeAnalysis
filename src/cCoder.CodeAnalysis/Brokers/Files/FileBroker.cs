// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Brokers.Files;

internal sealed class FileBroker : IFileBroker
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public IReadOnlyList<string> GetProjectFiles(string directoryPath)
    {
        return Directory
            .GetFiles(directoryPath, "*.csproj", SearchOption.TopDirectoryOnly)
            .OrderBy((string path) => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
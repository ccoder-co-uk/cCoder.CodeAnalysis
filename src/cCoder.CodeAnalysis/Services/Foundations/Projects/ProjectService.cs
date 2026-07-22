// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Brokers.Files;

namespace cCoder.CodeAnalysis.Services.Foundations.Projects;

internal sealed class ProjectService(IFileBroker fileBroker) : IProjectService
{
    public string ResolveProjectFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The project path is required.", nameof(path));
        }
        if (fileBroker.FileExists(path))
        {
            if (!string.Equals(Path.GetExtension(path), ".csproj", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The supplied file is not a .NET project file.", nameof(path));
            }
            return path;
        }
        if (!fileBroker.DirectoryExists(path))
        {
            throw new DirectoryNotFoundException("The project path '" + path + "' does not exist.");
        }
        IReadOnlyList<string> projectFiles = fileBroker.GetProjectFiles(path);
        switch (projectFiles.Count)
        {
            case 0:
                throw new FileNotFoundException("The supplied directory does not contain a .NET project file.", path);
            case 1:
                return projectFiles[0];
            default:
                throw new InvalidOperationException("The supplied directory contains more than one .NET project file.");
        }
    }
}
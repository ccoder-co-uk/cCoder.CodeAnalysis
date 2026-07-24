// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
namespace cCoder.CodeAnalysis.Brokers.Files;

internal interface IFileBroker
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    IReadOnlyList<string> GetProjectFiles(string directoryPath);
}
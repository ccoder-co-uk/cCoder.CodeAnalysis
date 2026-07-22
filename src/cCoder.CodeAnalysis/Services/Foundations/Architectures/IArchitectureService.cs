// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Foundations.Architectures;

internal interface IArchitectureService
{
    Architecture Build(string projectFilePath);
}
// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Architectures;

internal interface IArchitectureOrchestrationService
{
    Architecture Generate(string path);
}
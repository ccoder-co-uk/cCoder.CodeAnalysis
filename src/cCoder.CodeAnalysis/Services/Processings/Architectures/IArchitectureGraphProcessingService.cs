// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Architectures;

internal interface IArchitectureGraphProcessingService : ICodeAnalysisInfrastructureService
{
    ArchitectureBuild Process(ArchitectureBuild architectureBuild);
}

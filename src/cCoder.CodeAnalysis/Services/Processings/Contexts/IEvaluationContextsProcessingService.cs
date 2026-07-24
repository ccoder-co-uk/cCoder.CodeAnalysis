// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Contexts;

internal interface IEvaluationContextsProcessingService : ICodeAnalysisInfrastructureService
{
    IEnumerable<EvaluationContext> Process(ArchitectureBuild architectureBuild);
}
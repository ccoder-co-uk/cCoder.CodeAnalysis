// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Contexts;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Architectures;

internal sealed class ArchitectureOrchestrationService(
    IArchitectureProcessingService architectureProcessingService,
    IEvaluationContextsProcessingService evaluationContextsProcessingService,
    IRuleEvaluationsProcessingService ruleEvaluationsProcessingService
) : IArchitectureOrchestrationService
{
    public Architecture Generate(string path)
    {
        return Complete(architectureBuild: architectureProcessingService.Process(path: path));
    }

    public Architecture Generate(CSharpCompilation compilation) =>

        Complete(architectureBuild: architectureProcessingService.Process(compilation: compilation));

    private Architecture Complete(ArchitectureBuild architectureBuild)
    {
        IEnumerable<EvaluationContext> evaluationContexts = evaluationContextsProcessingService.Process(
            architectureBuild: architectureBuild
        );

        architectureBuild.Architecture.AnalysisItems = ruleEvaluationsProcessingService
            .Process(contexts: evaluationContexts)
            .ToList();

        return architectureBuild.Architecture;
    }
}
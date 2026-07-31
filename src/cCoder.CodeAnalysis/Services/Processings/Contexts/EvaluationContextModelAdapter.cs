// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Contexts;

internal static class EvaluationContextModelAdapter
{
    internal static EvaluationContext Attach(
        EvaluationContext context,
        Architecture architecture)
    {
        Class? architectureElement = architecture.Classes.SingleOrDefault(
            predicate: element => string.Equals(
                a: element.Name,
                b: context.TypeName,
                comparisonType: StringComparison.Ordinal));

        if (architectureElement is null)
        {
            return context;
        }

        architectureElement.AnalysisDependencies = context.Dependencies;
        architectureElement.AnalysisImplementedInterfaces = context.ImplementedInterfaces;
        architectureElement.AnalysisSourceFileTopLevelClassCount =
            context.SourceFileTopLevelClassCount;
        architectureElement.AnalysisIsPrimaryTopLevelClassInFile =
            context.IsPrimaryTopLevelClassInFile;
        context.ArchitectureModel = architecture;
        context.ArchitectureElement = architectureElement;

        return context;
    }
}

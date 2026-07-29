// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

namespace cCoder.CodeAnalysis.Sample.Brokers.RuleViolations;

internal sealed class InvalidBroker(HttpClient httpClient, Random random, IStudentService studentService)
{
    internal void Execute(bool shouldExecute)
    {
        if (shouldExecute)
        {
            studentService.GetStudents();
        }

        for (int index = 0; index < 1; index++)
        {
            httpClient.CancelPendingRequests();
        }

        try
        {
            random.Next();
        }
        catch (InvalidOperationException)
        {
        }
    }
}
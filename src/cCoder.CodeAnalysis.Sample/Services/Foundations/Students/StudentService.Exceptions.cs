// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Exceptions;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

internal sealed partial class StudentService
{
    private static void TryCatch(Action operation)
    {
        try
        {
            operation();
        }
        catch (ArgumentException innerException)
        {
            throw new StudentServiceValidationException(innerException);
        }
        catch (InvalidOperationException innerException2)
        {
            throw new StudentServiceDependencyException(innerException2);
        }
        catch (Exception innerException3)
        {
            throw new StudentServiceException(innerException3);
        }
    }

    private static T TryCatch<T>(Func<T> operation)
    {
        try
        {
            return operation();
        }
        catch (ArgumentException innerException)
        {
            throw new StudentServiceValidationException(innerException);
        }
        catch (InvalidOperationException innerException2)
        {
            throw new StudentServiceDependencyException(innerException2);
        }
        catch (Exception innerException3)
        {
            throw new StudentServiceException(innerException3);
        }
    }

    private static async ValueTask TryCatch(Func<ValueTask> operation)
    {
        try
        {
            await operation();
        }
        catch (ArgumentException innerException)
        {
            throw new StudentServiceValidationException(innerException);
        }
        catch (InvalidOperationException innerException)
        {
            throw new StudentServiceDependencyException(innerException);
        }
        catch (Exception innerException)
        {
            throw new StudentServiceException(innerException);
        }
    }

    private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException innerException)
        {
            throw new StudentServiceValidationException(innerException);
        }
        catch (InvalidOperationException innerException)
        {
            throw new StudentServiceDependencyException(innerException);
        }
        catch (Exception innerException)
        {
            throw new StudentServiceException(innerException);
        }
    }
}
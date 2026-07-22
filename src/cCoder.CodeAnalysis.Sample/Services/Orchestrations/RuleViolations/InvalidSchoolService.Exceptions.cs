// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Exceptions;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.RuleViolations;

internal sealed partial class InvalidSchoolService
{
private static void TryCatch(Action operation)
	{
		try
		{
			operation();
		}
		catch (ArgumentException innerException)
		{
			throw new ServiceValidationException(innerException);
		}
		catch (InvalidOperationException innerException2)
		{
			throw new ServiceDependencyException(innerException2);
		}
		catch (Exception innerException3)
		{
			throw new ServiceException(innerException3);
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
			throw new ServiceValidationException(innerException);
		}
		catch (InvalidOperationException innerException2)
		{
			throw new ServiceDependencyException(innerException2);
		}
		catch (Exception innerException3)
		{
			throw new ServiceException(innerException3);
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
		throw new ServiceValidationException(innerException);
	}
		catch (InvalidOperationException innerException)
	{
		throw new ServiceDependencyException(innerException);
	}
		catch (Exception innerException)
	{
		throw new ServiceException(innerException);
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
		throw new ServiceValidationException(innerException);
	}
		catch (InvalidOperationException innerException)
	{
		throw new ServiceDependencyException(innerException);
	}
		catch (Exception innerException)
	{
		throw new ServiceException(innerException);
	}
	}
}
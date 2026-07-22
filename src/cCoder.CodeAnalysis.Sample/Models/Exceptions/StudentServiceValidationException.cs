// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Exceptions;

public sealed class StudentServiceValidationException(Exception innerException) : ServiceValidationException(innerException)
{
}
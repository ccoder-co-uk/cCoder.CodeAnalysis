// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Exceptions;

public sealed class StudentServiceDependencyException(Exception innerException) : ServiceDependencyException(innerException)
{
}
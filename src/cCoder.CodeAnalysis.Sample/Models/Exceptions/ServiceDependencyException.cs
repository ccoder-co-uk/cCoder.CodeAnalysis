// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Exceptions;

public class ServiceDependencyException(Exception innerException) : InvalidOperationException("Service dependency failed.", innerException)
{
}
// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Exceptions;

public class ServiceValidationException(Exception innerException) : ArgumentException("Service validation failed.", innerException)
{
}
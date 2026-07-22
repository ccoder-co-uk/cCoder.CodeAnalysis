// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Exceptions;

public class ServiceException(Exception innerException) : Exception("Service failed.", innerException)
{
}
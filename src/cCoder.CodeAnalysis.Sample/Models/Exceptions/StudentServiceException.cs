// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Exceptions;

public sealed class StudentServiceException(Exception innerException) : ServiceException(innerException)
{
}
// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;

namespace cCoder.CodeAnalysis.Sample.Brokers.Loggings;

public interface ILoggingBroker : IUtilityBroker
{
    void LogError(Exception exception);
}
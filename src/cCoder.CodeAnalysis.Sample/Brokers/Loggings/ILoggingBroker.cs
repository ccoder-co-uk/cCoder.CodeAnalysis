// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;

namespace cCoder.CodeAnalysis.Sample.Brokers.Loggings;

public interface ILoggingBroker : IUtilityBroker
{
    void LogInformation(string message);

    void LogWarning(Exception exception);

    void LogWarning(string message);

    void LogError(Exception exception);
}
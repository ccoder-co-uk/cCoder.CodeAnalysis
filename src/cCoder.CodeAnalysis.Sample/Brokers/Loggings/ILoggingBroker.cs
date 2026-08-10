// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Brokers.Loggings;

    public interface ILoggingBroker
{
    void LogError(Exception exception);
}
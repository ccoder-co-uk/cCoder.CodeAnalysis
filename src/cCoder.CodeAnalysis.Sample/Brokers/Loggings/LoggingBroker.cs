// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Brokers.Loggings;

internal sealed class LoggingBroker(ILogger<LoggingBroker> logger)
    : ILoggingBroker
{
    public void LogInformation(string message) =>
        logger.LogInformation(message: message);

    public void LogWarning(Exception exception) =>
        logger.LogWarning(
            exception: exception,
            message: "An exposure operation was rejected.");

    public void LogWarning(string message) =>
        logger.LogWarning(message: message);

    public void LogError(Exception exception) =>
        logger.LogError(
            exception: exception,
            message: "An exposure operation failed.");
}
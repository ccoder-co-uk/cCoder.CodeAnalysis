// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Hosting;

namespace School.Cli.BadHost;

public static class IHostExtensions
{
    public static Task RunCommandAsync(
        this IHost host,
        string[] arguments) => Task.CompletedTask;
}

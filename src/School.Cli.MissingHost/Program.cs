// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace School.Cli.MissingHost;

public static class Program
{
    public static Task Main(string[] args) => RootCommand.RunAsync(args);

    private static class RootCommand
    {
        public static Task RunAsync(string[] args) => Task.CompletedTask;
    }
}

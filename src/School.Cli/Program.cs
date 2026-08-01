// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace School.Cli;

public static class Program
{
    public static void Configure(
        IConfiguration configuration,
        IServiceCollection services)
    {
        SchoolCliConfiguration applicationConfiguration = new();
        configuration.Bind(applicationConfiguration);
        services.AddCli(configuration);
    }
}

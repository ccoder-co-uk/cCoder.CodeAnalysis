// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace School.Cli
{
    public static class IServiceCollectionExtensions
    {
        public static void AddCli(
            this IServiceCollection services,
            IConfiguration applicationConfiguration)
        {
            services
                .AddScoped<DomainConfiguration>()
                .AddSingleton<object>();
        }

        public static void NotAnExtension() { }
    }
}

namespace School.Cli.MissingRegistration
{
    public static class IServiceCollectionExtensions
    {
        public static void Register(this IServiceCollection services) { }
    }
}

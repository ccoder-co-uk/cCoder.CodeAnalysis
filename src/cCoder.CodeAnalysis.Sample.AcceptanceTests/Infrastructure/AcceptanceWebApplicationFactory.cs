// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;

namespace cCoder.CodeAnalysis.Sample.AcceptanceTests.Infrastructure;

internal sealed class AcceptanceWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        string connectionString = Environment.GetEnvironmentVariable(
            variable: "CodeAnalysisSample__ConnectionString")
            ?? throw new InvalidOperationException(
                message:
                    "CodeAnalysisSample__ConnectionString is required.");

        SqlConnectionStringBuilder connectionStringBuilder =
            new(connectionString);

        connectionStringBuilder.InitialCatalog =
            $"{connectionStringBuilder.InitialCatalog}" +
            $"-acceptance-{Guid.NewGuid():N}";

        builder.UseSetting(
            key: "CodeAnalysisSample:ConnectionString",
            value: connectionStringBuilder.ConnectionString);
    }
}
// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCodeAnalysisSample(
    connectionString: builder.Configuration.GetConnectionString(name: "Students") ?? string.Empty
);

WebApplication application = builder.Build();

application.MapControllers();
application.MapDefaultControllerRoute();
application.Run();

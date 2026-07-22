// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Controllers;
using cCoder.CodeAnalysis.Sample.Exposures.ServiceCollections;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddControllersWithViews(options =>
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true
    )
    .AddApplicationPart(typeof(StudentsController).Assembly);

builder.Services.AddCodeAnalysisSample(
    connectionString: builder.Configuration.GetConnectionString(name: "Students") ?? string.Empty
);

WebApplication application = builder.Build();

application.MapControllers();
application.MapDefaultControllerRoute();
application.Run();
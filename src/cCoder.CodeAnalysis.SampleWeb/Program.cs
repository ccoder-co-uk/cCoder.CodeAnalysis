// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.SampleWeb;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSampleWeb(builder.Configuration);

WebApplication application = builder.Build();

application.MapControllers();
application.MapDefaultControllerRoute();
application.Run();
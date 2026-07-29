// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Exposures.Courses;
using cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;
using cCoder.CodeAnalysis.Sample.Exposures.SchoolImports;
using cCoder.CodeAnalysis.Sample.Exposures.Schools;
using cCoder.CodeAnalysis.Sample.Exposures.Storage;
using cCoder.CodeAnalysis.Sample.Exposures.Students;
using cCoder.CodeAnalysis.Sample.Exposures.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Courses;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using cCoder.CodeAnalysis.Sample.Services.Processings.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using cCoder.Eventing;
using Microsoft.EntityFrameworkCore;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;

internal sealed partial class ServiceCollectionProcessingService : IServiceCollectionProcessingService
{
    public IServiceCollection AddCodeAnalysisSample(IServiceCollection services, string connectionString) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: [services, connectionString]);
            services.AddEventing();

            services.AddDbContextFactory<SchoolContext>(
                optionsAction: delegate (DbContextOptionsBuilder options)
                {
                    options.UseSqlServer(connectionString: connectionString);
                }
            );

            services.AddScoped<ISchoolContextFactory, SchoolContextFactory>();
            services.AddScoped<ISchoolBroker, SchoolBroker>();
            services.AddScoped<IStudentBroker, StudentBroker>();
            services.AddScoped<ITeacherBroker, TeacherBroker>();
            services.AddScoped<ICourseBroker, CourseBroker>();
            services.AddScoped<ISchoolService, SchoolService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IEntityEventService, EntityEventService>();
            services.AddScoped<IEventHandlerService, EventHandlerService>();
            services.AddScoped<ISampleEventHandlers, SampleEventHandlers>();
            services.AddScoped<ISchoolOrchestrationService, SchoolOrchestrationService>();
            services.AddScoped<IStudentOrchestrationService, StudentOrchestrationService>();
            services.AddScoped<ITeacherOrchestrationService, TeacherOrchestrationService>();
            services.AddScoped<ICourseOrchestrationService, CourseOrchestrationService>();
            services.AddScoped<IStudentProcessingService, StudentProcessingService>();
            services.AddScoped<ITeacherProcessingService, TeacherProcessingService>();
            services.AddScoped<ICourseProcessingService, CourseProcessingService>();
            services.AddScoped<ISchoolImportProcessingService, SchoolImportProcessingService>();
            services.AddScoped<ISchoolStructureImportOrchestrationService, SchoolStructureImportOrchestrationService>();
            services.AddScoped<ISchoolPeopleImportOrchestrationService, SchoolPeopleImportOrchestrationService>();
            services.AddScoped<ISchoolImportCoordinationService, SchoolImportCoordinationService>();
            services.AddScoped<ISchoolImportValidationCoordinationService, SchoolImportValidationCoordinationService>();
            services.AddScoped<ISchoolImportManagementService, SchoolImportManagementService>();
            services.AddScoped<ISchoolImportReadinessManagementService, SchoolImportReadinessManagementService>();
            services.AddScoped<ISchoolImportAggregationService, SchoolImportAggregationService>();
            services.AddScoped<ISchoolImportManager, SchoolImportManager>();
            services.AddScoped<ISchoolManager, SchoolManager>();
            services.AddScoped<IStudentManager, StudentManager>();
            services.AddScoped<ITeacherManager, TeacherManager>();
            services.AddScoped<ICourseManager, CourseManager>();
            return services;
        });
}
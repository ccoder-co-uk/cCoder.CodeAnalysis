// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using System.Security.Authentication;
using cCoder.CodeAnalysis.Sample.Brokers.Loggings;
using cCoder.CodeAnalysis.Sample.Controllers;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace cCoder.CodeAnalysis.Sample.AcceptanceTests.Controllers;

public sealed class RuntimeControllerResponseContractTests
{
    private const string SensitiveDetail = "sensitive-injected-detail";

    [Fact]
    public async Task EveryRuntimeAction_WhenSuccessful_ShouldLogInformationOnly()
    {
        // Given
        foreach (RuntimeAction action in CreateActions(exception: null))
        {
            RecordingLoggingBroker loggingBroker = new();

            // When
            IActionResult result = await action.InvokeAsync.Invoke(
                arg: loggingBroker);

            // Then
            GetStatusCode(result: result)
                .Should()
                .Be(expected: action.SuccessStatusCode, because: action.Name);

            VerifyLogCounts(
                loggingBroker: loggingBroker,
                information: 1,
                warning: 0,
                error: 0);
        }
    }

    [Fact]
    public async Task EveryRuntimeAction_WhenRejectedOrFailed_ShouldClassifyAndLogSafely()
    {
        // Given
        foreach (FailureOutcome outcome in CreateFailureOutcomes())
        {
            foreach (RuntimeAction action in CreateActions(
                exception: outcome.Exception))
            {
                RecordingLoggingBroker loggingBroker = new();

                // When
                IActionResult result = await action.InvokeAsync.Invoke(
                    arg: loggingBroker);

                // Then
                GetStatusCode(result: result)
                    .Should()
                    .Be(expected: outcome.StatusCode,
                        because: $"{action.Name} should classify {outcome.Name}");

                string responseBody = GetResponseBody(result: result);

                responseBody
                    .Should()
                    .Be(expected: outcome.SafeResponseBody);

                responseBody
                    .Should()
                    .NotContain(unexpected: SensitiveDetail);

                VerifyLogCounts(
                    loggingBroker: loggingBroker,
                    information: 0,
                    warning: outcome.WarningCount,
                    error: outcome.ErrorCount);
            }
        }
    }

    [Fact]
    public void GetStudent_WhenMissing_ShouldReturnNotFoundAndLogWarningOnly()
    {
        // Given
        RecordingLoggingBroker loggingBroker = new();

        StudentsController controller = new(
            studentOrchestrationService: new MissingStudentOrchestrationService(),
            loggingBroker: loggingBroker);

        // When
        IActionResult result = controller.GetStudent(studentId: 404).Result!;

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: StatusCodes.Status404NotFound);

        VerifyLogCounts(
            loggingBroker: loggingBroker,
            information: 0,
            warning: 1,
            error: 0);
    }

    [Fact]
    public async Task DeleteStudent_WhenMissing_ShouldReturnNotFoundAndLogWarningOnly()
    {
        // Given
        RecordingLoggingBroker loggingBroker = new();

        StudentsController controller = new(
            studentOrchestrationService: new MissingStudentOrchestrationService(),
            loggingBroker: loggingBroker);

        // When
        IActionResult result = await controller.DeleteStudentAsync(studentId: 404);

        // Then
        GetStatusCode(result: result)
            .Should()
            .Be(expected: StatusCodes.Status404NotFound);

        VerifyLogCounts(
            loggingBroker: loggingBroker,
            information: 0,
            warning: 1,
            error: 0);
    }

    private static IEnumerable<RuntimeAction> CreateActions(Exception? exception)
    {
        IStudentOrchestrationService studentService = exception is null
            ? new SuccessfulStudentOrchestrationService()
            : new ThrowingStudentOrchestrationService(exception: exception);

        ISchoolImportAggregationService schoolImportService = exception is null
            ? new SuccessfulSchoolImportAggregationService()
            : new ThrowingSchoolImportAggregationService(exception: exception);

        yield return new RuntimeAction(
            Name: "Students.GetStudents",
            SuccessStatusCode: StatusCodes.Status200OK,
            InvokeAsync: loggingBroker => ValueTask.FromResult(
                result: (IActionResult)new StudentsController(
                    studentOrchestrationService: studentService,
                    loggingBroker: loggingBroker)
                    .GetStudents()
                    .Result!));

        yield return new RuntimeAction(
            Name: "Students.GetStudent",
            SuccessStatusCode: StatusCodes.Status200OK,
            InvokeAsync: loggingBroker => ValueTask.FromResult(
                result: (IActionResult)new StudentsController(
                    studentOrchestrationService: studentService,
                    loggingBroker: loggingBroker)
                    .GetStudent(studentId: 1)
                    .Result!));

        yield return new RuntimeAction(
            Name: "Students.PostStudentAsync",
            SuccessStatusCode: StatusCodes.Status201Created,
            InvokeAsync: async loggingBroker => (await new StudentsController(
                studentOrchestrationService: studentService,
                loggingBroker: loggingBroker)
                .PostStudentAsync(newStudent: new Student()))
                .Result!);

        yield return new RuntimeAction(
            Name: "Students.PutStudentAsync",
            SuccessStatusCode: StatusCodes.Status200OK,
            InvokeAsync: async loggingBroker => (await new StudentsController(
                studentOrchestrationService: studentService,
                loggingBroker: loggingBroker)
                .PutStudentAsync(updatedStudent: new Student()))
                .Result!);

        yield return new RuntimeAction(
            Name: "Students.DeleteStudentAsync",
            SuccessStatusCode: StatusCodes.Status204NoContent,
            InvokeAsync: loggingBroker => new StudentsController(
                studentOrchestrationService: studentService,
                loggingBroker: loggingBroker)
                .DeleteStudentAsync(studentId: 1));

        yield return new RuntimeAction(
            Name: "SchoolImport.PostSchoolAsync",
            SuccessStatusCode: StatusCodes.Status202Accepted,
            InvokeAsync: loggingBroker => new SchoolImportController(
                schoolImportAggregationService: schoolImportService,
                loggingBroker: loggingBroker)
                .PostSchoolAsync(newSchool: new School()));
    }

    private static IEnumerable<FailureOutcome> CreateFailureOutcomes()
    {
        yield return new FailureOutcome(
            Name: "typed validation",
            Exception: new ServiceValidationException(
                innerException: new ArgumentException(message: SensitiveDetail)),
            StatusCode: StatusCodes.Status400BadRequest,
            SafeResponseBody: "The request is invalid.",
            WarningCount: 1,
            ErrorCount: 0);

        yield return new FailureOutcome(
            Name: "argument validation",
            Exception: new ArgumentException(message: SensitiveDetail),
            StatusCode: StatusCodes.Status400BadRequest,
            SafeResponseBody: "The request is invalid.",
            WarningCount: 1,
            ErrorCount: 0);

        yield return new FailureOutcome(
            Name: "authentication",
            Exception: new AuthenticationException(message: SensitiveDetail),
            StatusCode: StatusCodes.Status401Unauthorized,
            SafeResponseBody: "Authentication failed.",
            WarningCount: 1,
            ErrorCount: 0);

        yield return new FailureOutcome(
            Name: "authorization",
            Exception: new UnauthorizedAccessException(message: SensitiveDetail),
            StatusCode: StatusCodes.Status403Forbidden,
            SafeResponseBody: "Access is denied.",
            WarningCount: 1,
            ErrorCount: 0);

        yield return new FailureOutcome(
            Name: "security",
            Exception: new SecurityException(message: SensitiveDetail),
            StatusCode: StatusCodes.Status403Forbidden,
            SafeResponseBody: "Access is denied.",
            WarningCount: 1,
            ErrorCount: 0);

        yield return new FailureOutcome(
            Name: "unexpected",
            Exception: new Exception(message: SensitiveDetail),
            StatusCode: StatusCodes.Status500InternalServerError,
            SafeResponseBody: "The operation failed.",
            WarningCount: 0,
            ErrorCount: 1);
    }

    private static int GetStatusCode(IActionResult result) =>
        ((IStatusCodeActionResult)result).StatusCode
            ?? StatusCodes.Status200OK;

    private static string GetResponseBody(IActionResult result) =>
        ((ObjectResult)result).Value?.ToString() ?? string.Empty;

    private static void VerifyLogCounts(
        RecordingLoggingBroker loggingBroker,
        int information,
        int warning,
        int error)
    {
        loggingBroker.InformationCount
            .Should()
            .Be(expected: information);

        loggingBroker.WarningCount
            .Should()
            .Be(expected: warning);

        loggingBroker.ErrorCount
            .Should()
            .Be(expected: error);
    }

    private sealed record RuntimeAction(
        string Name,
        int SuccessStatusCode,
        Func<ILoggingBroker, ValueTask<IActionResult>> InvokeAsync);

    private sealed record FailureOutcome(
        string Name,
        Exception Exception,
        int StatusCode,
        string SafeResponseBody,
        int WarningCount,
        int ErrorCount);

    private sealed class RecordingLoggingBroker : ILoggingBroker
    {
        public int InformationCount { get; private set; }

        public int WarningCount { get; private set; }

        public int ErrorCount { get; private set; }

        public void LogInformation(string message) => InformationCount++;

        public void LogWarning(Exception exception) => WarningCount++;

        public void LogWarning(string message) => WarningCount++;

        public void LogError(Exception exception) => ErrorCount++;
    }

    private sealed class SuccessfulStudentOrchestrationService :
        IStudentOrchestrationService
    {
        private readonly Student student = new() { Id = 1 };

        public Student GetStudent(int studentId) => student;

        public IQueryable<Student> GetStudents() =>
            new[] { student }.AsQueryable();

        public ValueTask<Student> AddStudentAsync(Student newStudent) =>
            ValueTask.FromResult(result: newStudent);

        public ValueTask<Student> UpdateStudentAsync(Student updatedStudent) =>
            ValueTask.FromResult(result: updatedStudent);

        public ValueTask DeleteStudentAsync(int studentId) =>
            ValueTask.CompletedTask;
    }

    private sealed class MissingStudentOrchestrationService :
        IStudentOrchestrationService
    {
        public Student? GetStudent(int studentId) => null;

        public IQueryable<Student> GetStudents() =>
            Array.Empty<Student>().AsQueryable();

        public ValueTask<Student> AddStudentAsync(Student newStudent) =>
            ValueTask.FromResult(result: newStudent);

        public ValueTask<Student> UpdateStudentAsync(Student updatedStudent) =>
            ValueTask.FromResult(result: updatedStudent);

        public ValueTask DeleteStudentAsync(int studentId) =>
            ValueTask.CompletedTask;
    }

    private sealed class ThrowingStudentOrchestrationService(Exception exception) :
        IStudentOrchestrationService
    {
        public Student GetStudent(int studentId) => throw exception;

        public IQueryable<Student> GetStudents() => throw exception;

        public ValueTask<Student> AddStudentAsync(Student newStudent) =>
            ValueTask.FromException<Student>(exception: exception);

        public ValueTask<Student> UpdateStudentAsync(Student updatedStudent) =>
            ValueTask.FromException<Student>(exception: exception);

        public ValueTask DeleteStudentAsync(int studentId) =>
            ValueTask.FromException(exception: exception);
    }

    private sealed class SuccessfulSchoolImportAggregationService :
        ISchoolImportAggregationService
    {
        public ValueTask ImportSchoolAsync(School school) =>
            ValueTask.CompletedTask;
    }

    private sealed class ThrowingSchoolImportAggregationService(Exception exception) :
        ISchoolImportAggregationService
    {
        public ValueTask ImportSchoolAsync(School school) =>
            ValueTask.FromException(exception: exception);
    }
}
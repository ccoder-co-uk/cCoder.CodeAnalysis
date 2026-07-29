// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Json;
using System.Text;
using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.AcceptanceTests.Infrastructure;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Sample.AcceptanceTests.Controllers.Students;

public sealed class StudentsControllerAcceptanceTests : IAsyncLifetime
{
    private readonly AcceptanceWebApplicationFactory applicationFactory = new();

    private HttpClient client = null!;

    private int schoolId;

    public async Task InitializeAsync()
    {
        client = applicationFactory.CreateClient();
        await using AsyncServiceScope scope = applicationFactory.Services.CreateAsyncScope();
        IDbContextFactory<SchoolContext> contextFactory = scope.ServiceProvider.GetRequiredService<
            IDbContextFactory<SchoolContext>
        >();
        await using SchoolContext context = await contextFactory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        School school = new School { Name = $"Acceptance School {Guid.NewGuid():N}" };
        context.Schools.Add(school);
        await context.SaveChangesAsync();
        schoolId = school.Id;
    }

    public async Task DisposeAsync()
    {
        await using AsyncServiceScope scope = applicationFactory.Services.CreateAsyncScope();
        IDbContextFactory<SchoolContext> contextFactory = scope.ServiceProvider.GetRequiredService<
            IDbContextFactory<SchoolContext>
        >();
        await using SchoolContext context = await contextFactory.CreateDbContextAsync();
        Student[] students = await context
            .Students.Where((Student student) => student.SchoolId == schoolId)
            .ToArrayAsync();
        context.Students.RemoveRange(students);
        School? school = await context.Schools.FindAsync(schoolId);
        if (school != null)
        {
            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }

        await context.Database.EnsureDeletedAsync();
        client.Dispose();
        await applicationFactory.DisposeAsync();
    }

    private Student CreateStudent()
    {
        return new Student
        {
            FirstName = "Ada",
            LastName = $"Lovelace-{Guid.NewGuid():N}",
            SchoolId = schoolId,
            School = new School { Id = schoolId, Name = "Acceptance School" },
        };
    }

    private async ValueTask<Student> PostStudentAsync(Student newStudent)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/students", newStudent);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Student>())!;
    }

    private WebApplicationFactory<Program> CreateUnavailableDatabaseApplicationFactory()
    {
        return applicationFactory.WithWebHostBuilder(
            delegate (IWebHostBuilder builder)
            {
                builder.UseSetting(
                    "CodeAnalysisSample:ConnectionString",
                    "Server=127.0.0.1,1;Database=Unavailable;User Id=invalid;Password=invalid;TrustServerCertificate=True;Connect Timeout=1"
                );
            }
        );
    }

    [Theory]
    [InlineData(new object[] { "/api/students" })]
    [InlineData(new object[] { "/api/students/7" })]
    public async Task GetStudentEndpointsReturnServerErrorWhenDatabaseIsUnavailable(string requestUri)
    {
        await using WebApplicationFactory<Program> factory = CreateUnavailableDatabaseApplicationFactory();
        using HttpClient unavailableClient = factory.CreateClient();
        EnumAssertionsExtensions
            .Should((await unavailableClient.GetAsync(requestUri)).StatusCode)
            .Be(HttpStatusCode.InternalServerError, "");
    }

    [Fact]
    public async Task DeleteStudentAsyncReturnsServerErrorWhenDatabaseIsUnavailable()
    {
        await using WebApplicationFactory<Program> factory = CreateUnavailableDatabaseApplicationFactory();
        using HttpClient unavailableClient = factory.CreateClient();
        EnumAssertionsExtensions
            .Should((await unavailableClient.DeleteAsync("/api/students/7")).StatusCode)
            .Be(HttpStatusCode.InternalServerError, "");
    }

    [Fact]
    public async Task DeleteStudentAsyncRemovesStudent()
    {
        Student newStudent = CreateStudent();
        Student postedStudent = await PostStudentAsync(newStudent);
        HttpResponseMessage deleteResponse = await client.DeleteAsync($"/api/students/{postedStudent.Id}");
        HttpResponseMessage getResponse = await client.GetAsync($"/api/students/{postedStudent.Id}");
        EnumAssertionsExtensions.Should(deleteResponse.StatusCode).Be(HttpStatusCode.NoContent, "");
        EnumAssertionsExtensions.Should(getResponse.StatusCode).Be(HttpStatusCode.NotFound, "");
    }

    [Fact]
    public async Task DeleteStudentAsyncReturnsNotFoundForMissingStudent()
    {
        EnumAssertionsExtensions
            .Should((await client.DeleteAsync($"/api/students/{int.MaxValue}")).StatusCode)
            .Be(HttpStatusCode.NotFound, "");
    }

    [Fact]
    public async Task GetStudentReturnsPersistedStudent()
    {
        Student newStudent = CreateStudent();
        Student postedStudent = await PostStudentAsync(newStudent);
        Student? retrievedStudent = await client.GetFromJsonAsync<Student>($"/api/students/{postedStudent.Id}");
        ((object?)retrievedStudent).Should().NotBeNull("");
        retrievedStudent.Id.Should().Be(postedStudent.Id, "");
    }

    [Fact]
    public async Task GetStudentReturnsNotFoundForMissingStudent()
    {
        EnumAssertionsExtensions
            .Should((await client.GetAsync($"/api/students/{int.MaxValue}")).StatusCode)
            .Be(HttpStatusCode.NotFound, "");
    }

    [Fact]
    public async Task GetStudentsReturnsPersistedStudent()
    {
        Student newStudent = CreateStudent();
        Student postedStudent = await PostStudentAsync(newStudent);
        ((IEnumerable<Student>)((await client.GetFromJsonAsync<Student[]>("/api/students")) ?? []))
            .Should()
            .ContainSingle((Student student) => student.Id == postedStudent.Id, "");
    }

    [Fact]
    public async Task PostStudentAsyncPersistsStudent()
    {
        Student newStudent = CreateStudent();
        Student postedStudent = await PostStudentAsync(newStudent);
        Student? retrievedStudent = await client.GetFromJsonAsync<Student>($"/api/students/{postedStudent.Id}");
        postedStudent.Id.Should().BeGreaterThan(0, "");
        ((object?)retrievedStudent).Should().NotBeNull("");
        retrievedStudent.Id.Should().Be(postedStudent.Id, "");
        retrievedStudent.FirstName.Should().Be(postedStudent.FirstName, "");
        retrievedStudent.LastName.Should().Be(postedStudent.LastName, "");
        retrievedStudent.SchoolId.Should().Be(postedStudent.SchoolId, "");
    }

    [Fact]
    public async Task PostStudentAsyncReturnsServerErrorForPersistenceFailure()
    {
        Student newStudent = CreateStudent();
        newStudent.SchoolId = int.MaxValue;
        newStudent.School.Id = int.MaxValue;
        EnumAssertionsExtensions
            .Should((await client.PostAsJsonAsync("/api/students", newStudent)).StatusCode)
            .Be(HttpStatusCode.InternalServerError, "");
    }

    [Fact]
    public async Task PostStudentAsyncReturnsBadRequestForMissingStudent()
    {
        using StringContent content = new StringContent("null", Encoding.UTF8, "application/json");
        EnumAssertionsExtensions
            .Should((await client.PostAsync("/api/students", content)).StatusCode)
            .Be(HttpStatusCode.BadRequest, "");
    }

    [Fact]
    public async Task PutStudentAsyncPersistsStudentChanges()
    {
        Student newStudent = CreateStudent();
        Student postedStudent = await PostStudentAsync(newStudent);
        postedStudent.FirstName = "Grace";
        (await client.PutAsJsonAsync("/api/students", postedStudent)).EnsureSuccessStatusCode();
        Student? retrievedStudent = await client.GetFromJsonAsync<Student>($"/api/students/{postedStudent.Id}");
        ((object?)retrievedStudent).Should().NotBeNull("");
        retrievedStudent.FirstName.Should().Be("Grace", "");
    }

    [Fact]
    public async Task PutStudentAsyncReturnsServerErrorForPersistenceFailure()
    {
        Student updatedStudent = CreateStudent();
        updatedStudent.Id = int.MaxValue;
        EnumAssertionsExtensions
            .Should((await client.PutAsJsonAsync("/api/students", updatedStudent)).StatusCode)
            .Be(HttpStatusCode.InternalServerError, "");
    }

    [Fact]
    public async Task PutStudentAsyncReturnsBadRequestForMissingStudent()
    {
        using StringContent content = new StringContent("null", Encoding.UTF8, "application/json");
        EnumAssertionsExtensions
            .Should((await client.PutAsync("/api/students", content)).StatusCode)
            .Be(HttpStatusCode.BadRequest, "");
    }
}
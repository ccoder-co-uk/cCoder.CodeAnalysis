// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.AcceptanceTests.Infrastructure;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Sample.AcceptanceTests.Controllers.SchoolImports;

public sealed class SchoolImportControllerAcceptanceTests : IAsyncLifetime
{
    private readonly AcceptanceWebApplicationFactory applicationFactory = new();

    private HttpClient client = null!;

    private int importedSchoolId;

    public async Task InitializeAsync()
    {
        client = applicationFactory.CreateClient();
        await using AsyncServiceScope scope = applicationFactory.Services.CreateAsyncScope();

        IDbContextFactory<SchoolContext> contextFactory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<SchoolContext>>();

        await using SchoolContext context = await contextFactory
            .CreateDbContextAsync();

        await context.Database
            .EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using AsyncServiceScope scope = applicationFactory.Services.CreateAsyncScope();
        IDbContextFactory<SchoolContext> contextFactory = scope.ServiceProvider.GetRequiredService<
            IDbContextFactory<SchoolContext>
        >();
        await using SchoolContext context = await contextFactory.CreateDbContextAsync();
        School? school = await context
            .Schools.Include((School school2) => school2.Students)
            .Include((School school2) => school2.Teachers)
            .FirstOrDefaultAsync((School school2) => school2.Id == importedSchoolId);
        if (school != null)
        {
            context.Students.RemoveRange(school.Students);
            context.Teachers.RemoveRange(school.Teachers);
            context.Schools.Remove(school);
            await context.SaveChangesAsync();
        }

        await context.Database.EnsureDeletedAsync();
        client.Dispose();
        await applicationFactory.DisposeAsync();
    }

    private static School CreateSchool()
    {
        School obj = new School
        {
            Name = $"Imported School {Guid.NewGuid():N}",
            Courses = [],
        };
        int num = 1;
        List<Student> list = new List<Student>(num);
        CollectionsMarshal.SetCount(list, num);
        CollectionsMarshal.AsSpan(list)[0] = new Student { FirstName = "Katherine", LastName = "Johnson" };
        obj.Students = list;
        num = 1;
        List<Teacher> list2 = new List<Teacher>(num);
        CollectionsMarshal.SetCount(list2, num);
        CollectionsMarshal.AsSpan(list2)[0] = new Teacher { FirstName = "Mary", LastName = "Jackson" };
        obj.Teachers = list2;
        return obj;
    }

    [Fact]
    public async Task PostSchoolAsyncPersistsSchoolGraph()
    {
        School newSchool = CreateSchool();
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/schools/import", newSchool);
        await using AsyncServiceScope scope = applicationFactory.Services.CreateAsyncScope();
        IDbContextFactory<SchoolContext> contextFactory = scope.ServiceProvider.GetRequiredService<
            IDbContextFactory<SchoolContext>
        >();
        await using SchoolContext context = await contextFactory.CreateDbContextAsync();
        School? importedSchool = await context
            .Schools.Include((School school) => school.Students)
            .Include((School school) => school.Teachers)
            .SingleOrDefaultAsync((School school) => school.Name == newSchool.Name);
        importedSchoolId = importedSchool?.Id ?? 0;
        string responseContent = await response.Content.ReadAsStringAsync();
        EnumAssertionsExtensions.Should(response.StatusCode).Be(HttpStatusCode.Accepted, responseContent);
        ((object?)importedSchool).Should().NotBeNull("");
        ((IEnumerable<Student>)importedSchool!.Students).Should().ContainSingle("");
        ((IEnumerable<Teacher>)importedSchool.Teachers).Should().ContainSingle("");
    }

    [Fact]
    public async Task PostSchoolAsyncDoesNotPersistRejectedSchool()
    {
        School invalidSchool = CreateSchool();
        invalidSchool.Name = string.Empty;
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/schools/import", invalidSchool);
        await using AsyncServiceScope scope = applicationFactory.Services.CreateAsyncScope();
        IDbContextFactory<SchoolContext> contextFactory = scope.ServiceProvider.GetRequiredService<
            IDbContextFactory<SchoolContext>
        >();
        await using SchoolContext context = await contextFactory.CreateDbContextAsync();
        bool schoolExists = await context.Schools.AnyAsync((School school) => school.Name == invalidSchool.Name);
        EnumAssertionsExtensions.Should(response.StatusCode).Be(HttpStatusCode.Accepted, responseContent);
        schoolExists.Should().BeFalse("");
    }

    [Fact]
    public async Task PostSchoolAsyncReturnsServerErrorForPersistenceFailure()
    {
        School school = CreateSchool();
        school.Courses = new List<Course>(1)
        {
            new Course { Name = "Invalid Course", TeacherId = int.MaxValue },
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/schools/import", school);
        await using AsyncServiceScope scope = applicationFactory.Services.CreateAsyncScope();
        IDbContextFactory<SchoolContext> contextFactory = scope.ServiceProvider.GetRequiredService<
            IDbContextFactory<SchoolContext>
        >();
        await using SchoolContext context = await contextFactory.CreateDbContextAsync();
        importedSchoolId = await context.Schools
            .Where(
                (School storedSchool) =>
                    storedSchool.Name == school.Name
            )
            .Select((School storedSchool) => storedSchool.Id)
            .SingleOrDefaultAsync();
        EnumAssertionsExtensions.Should(response.StatusCode).Be(HttpStatusCode.InternalServerError, "");
    }

    [Fact]
    public async Task PostSchoolAsyncReturnsBadRequestForMissingSchool()
    {
        using StringContent content = new StringContent("null", Encoding.UTF8, "application/json");
        EnumAssertionsExtensions
            .Should((await client.PostAsync("/api/schools/import", content)).StatusCode)
            .Be(HttpStatusCode.BadRequest, "");
    }
}

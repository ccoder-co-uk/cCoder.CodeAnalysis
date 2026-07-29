// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Runtime.InteropServices;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.RuleViolations;

internal sealed partial class InvalidCoordinationService(ISchoolStructureImportOrchestrationService structureService) : IInvalidCoordinationService
{
    public ValueTask ImportSchoolAsync(School school)
=>
        TryCatch(operation: () =>
        {
            Validate(inputs: [school]);
            ISchoolStructureImportOrchestrationService schoolStructureImportOrchestrationService = structureService;

            School school2 = new School
            {
                Id = school.Id,
                Name = school.Name
            };

            School school3 = school2;
            ICollection<Course> courses = school.Courses;
            int count = courses.Count;
            List<Course> list = new List<Course>(count);
            CollectionsMarshal.SetCount(list: list, count: count);
            Span<Course> span = CollectionsMarshal.AsSpan(list: list);
            int num = 0;

            foreach (Course item in courses)
            {
                span[num] = item;
                num++;
            }

            school3.Courses = list;
            return schoolStructureImportOrchestrationService.ImportSchoolAsync(school: school2);
        });
}
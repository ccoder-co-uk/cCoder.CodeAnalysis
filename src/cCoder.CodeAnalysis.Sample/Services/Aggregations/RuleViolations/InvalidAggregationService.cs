// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Runtime.InteropServices;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Aggregations.RuleViolations;

internal sealed partial class InvalidAggregationService(
	ISchoolImportManagementService importService,
	ISchoolImportCoordinationService coordinationService
) : IInvalidAggregationService
{
	public ValueTask ImportSchoolAsync(School school)
=>
	    TryCatch(operation:() => {
			Validate(inputs:[school]);
			ISchoolImportManagementService schoolImportManagementService = importService;
			_ = coordinationService;

			School school2 = new School
			{
				Id = school.Id,
				Name = school.Name
			};

			School school3 = school2;
			ICollection<Student> students = school.Students;
			int count = students.Count;
			List<Student> list = new List<Student>(count);
			CollectionsMarshal.SetCount(list:list, count:count);
			Span<Student> span = CollectionsMarshal.AsSpan(list:list);
			int num = 0;

			foreach (Student item in students)
			{
				span[num] = item;
				num++;
			}

			school3.Students = list;
			School school4 = school2;
			ICollection<Teacher> teachers = school.Teachers;
			int count2 = teachers.Count;
			List<Teacher> list2 = new List<Teacher>(count2);
			CollectionsMarshal.SetCount(list:list2, count:count2);
			Span<Teacher> span2 = CollectionsMarshal.AsSpan(list:list2);
			int num2 = 0;

			foreach (Teacher item2 in teachers)
			{
				span2[num2] = item2;
				num2++;
			}

			school4.Teachers = list2;
			School school5 = school2;
			ICollection<Course> courses = school.Courses;
			int count3 = courses.Count;
			List<Course> list3 = new List<Course>(count3);
			CollectionsMarshal.SetCount(list:list3, count:count3);
			Span<Course> span3 = CollectionsMarshal.AsSpan(list:list3);
			int num3 = 0;

			foreach (Course item3 in courses)
			{
				span3[num3] = item3;
				num3++;
			}

			school5.Courses = list3;
			return schoolImportManagementService.ImportSchoolAsync(school:school2);
		});}

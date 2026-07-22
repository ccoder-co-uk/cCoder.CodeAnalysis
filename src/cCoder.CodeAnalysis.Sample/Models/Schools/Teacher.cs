// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Schools;

public sealed class Teacher
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public int SchoolId { get; set; }

    public School School { get; set; } = null!;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
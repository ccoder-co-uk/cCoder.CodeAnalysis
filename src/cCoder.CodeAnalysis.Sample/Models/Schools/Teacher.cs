// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Schools;

#pragma warning disable CS8618
public sealed class Teacher
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public int SchoolId { get; set; }

    public School School { get; set; }

    public ICollection<Course> Courses { get; set; }
}
#pragma warning restore CS8618
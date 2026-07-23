// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Schools;

#pragma warning disable CS8618
public sealed class School
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Student> Students { get; set; }

    public ICollection<Teacher> Teachers { get; set; }

    public ICollection<Course> Courses { get; set; }
}
#pragma warning restore CS8618
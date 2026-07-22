// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Schools;

public sealed class School
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();

    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
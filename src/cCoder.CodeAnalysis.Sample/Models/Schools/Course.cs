// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Schools;

public sealed class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SchoolId { get; set; }

    public School School { get; set; } = null!;

    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
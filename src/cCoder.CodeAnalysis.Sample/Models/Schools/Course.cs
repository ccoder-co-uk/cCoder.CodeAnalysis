// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Schools;

#pragma warning disable CS8618
public sealed class Course
{
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the course name.
    /// </summary>
    public string Name { get; set; }

    public int SchoolId { get; set; }

    public School School { get; set; }

    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; }

    public ICollection<Student> Students { get; set; }
}
#pragma warning restore CS8618
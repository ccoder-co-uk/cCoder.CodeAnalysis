// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.EntityFrameworkCore;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class SchoolContext(DbContextOptions<SchoolContext> options) : DbContext(options)
{
    public DbSet<School> Schools => Set<School>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<School>()
            .HasMany(navigationExpression: (School school) => school.Students)
            .WithOne(navigationExpression: (Student student) => student.School)
            .HasForeignKey(foreignKeyExpression: (Student student) => student.SchoolId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        modelBuilder
            .Entity<School>()
            .HasMany(navigationExpression: (School school) => school.Teachers)
            .WithOne(navigationExpression: (Teacher teacher) => teacher.School)
            .HasForeignKey(foreignKeyExpression: (Teacher teacher) => teacher.SchoolId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        modelBuilder
            .Entity<School>()
            .HasMany(navigationExpression: (School school) => school.Courses)
            .WithOne(navigationExpression: (Course course) => course.School)
            .HasForeignKey(foreignKeyExpression: (Course course) => course.SchoolId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Teacher>()
            .HasMany(navigationExpression: (Teacher teacher) => teacher.Courses)
            .WithOne(navigationExpression: (Course course) => course.Teacher)
            .HasForeignKey(foreignKeyExpression: (Course course) => course.TeacherId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Student>()
            .HasMany(navigationExpression: (Student student) => student.Courses)
            .WithMany(navigationExpression: (Course course) => course.Students);
    }
}
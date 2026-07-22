// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Students;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(IStudentManager studentManager) : ControllerBase
{
    [HttpGet]
    public ActionResult<IQueryable<Student>> GetStudents()
    {
        return Ok(value: studentManager.GetStudents());
    }

    [HttpGet("{studentId:int}")]
    public ActionResult<Student> GetStudent(int studentId)
    {
        Student? student = studentManager.GetStudent(studentId: studentId);
        return (student == null) ? ((ActionResult<Student>)NotFound()) : ((ActionResult<Student>)Ok(value: student));
    }

    [HttpPost]
    public async ValueTask<ActionResult<Student>> PostStudentAsync(Student newStudent)
    {
        Student addedStudent = await studentManager.AddStudentAsync(newStudent: newStudent);

        return CreatedAtAction(
            actionName: "GetStudent",
            routeValues: new { studentId = addedStudent.Id },
            value: addedStudent
        );
    }

    [HttpPut]
    public async ValueTask<ActionResult<Student>> PutStudentAsync(Student updatedStudent)
    {
        return Ok(value: await studentManager.UpdateStudentAsync(updatedStudent: updatedStudent));
    }

    [HttpDelete("{studentId:int}")]
    public async ValueTask<IActionResult> DeleteStudentAsync(int studentId)
    {
        Student? student = studentManager.GetStudent(studentId: studentId);

        if (student == null)
        {
            return NotFound();
        }

        await studentManager.DeleteStudentAsync(studentId: studentId);
        return NoContent();
    }
}
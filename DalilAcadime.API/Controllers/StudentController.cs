using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DalilAcadime.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentController(IStudentService studentService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<PageResult<StudentResponseDto>>> GetStudentsPagenation([FromQuery] int page = 1 , [FromQuery] int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            return Ok(await studentService.GetStudentsPagenation(page, pageSize));
        }

        // GET /api/students/{id}/courses in 1 query
        [HttpGet("{id}/courses")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<List<CourseResponseDto>>> GetStudentCourses(int id)
        {
            return Ok(await studentService.GetStudentCourses(id));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<StudentResponseDto>> GetStudentById(int id)
        {
            return Ok(await studentService.GetStudentById(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<TokenizedStudentDto>> CreateStudent(CreateStudentDto dto)
        {

            var createdStudent = await studentService.CreateStudent(dto);
            return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.Id }, createdStudent);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<StudentResponseDto>> UpdateStudent(int id, UpdateStudentDto updatedStudent)
        {
            return Ok(await studentService.UpdateStudent(id, updatedStudent));             
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<Student>> DeleteStudent(int id)
        {
                return Ok(await studentService.DeleteStudent(id));
        }
    }
}

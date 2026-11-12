using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DalilAcadime.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentController(IEnrollmentService enrollmentService) : ControllerBase
    {
        [HttpGet("me")]
        [Authorize(Roles = UserRoles.Student)]
        public async Task<ActionResult<List<CourseResponseDto>>> GetMyCourses()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("The user identity is missing or invalid.");
            }

            return Ok(await enrollmentService.GetStudentCoursesByUserId(userId));
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<List<EnrollmentResponseDto>>> GetAll()
        {
            return Ok(await enrollmentService.GetAll());
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<EnrollmentResponseDto>> GetById(int id)
        {
            return Ok(await enrollmentService.GetById(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<EnrollmentResponseDto>> Create(CreateEnrollmentDto dto)
        {
            var created = await enrollmentService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<EnrollmentResponseDto>> Update(int id, UpdateEnrollmentDto dto)
        {
            return Ok(await enrollmentService.Update(id, dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<EnrollmentResponseDto>> Delete(int id)
        {
            return Ok(await enrollmentService.Delete(id));
        }
    }
}

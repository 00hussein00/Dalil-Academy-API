using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Services;
using DalilAcadime.Core.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DalilAcadime.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController(
        ICourseServices courseService,
        IImageService imageService
        ) : ControllerBase
    {
        [HttpGet] 
        [AllowAnonymous]
        public async Task<ActionResult<PageResult<CourseResponseDto>>> GetCoursesPagenation([FromQuery] int page = 1, [FromQuery] int pageSize = 10 , [FromQuery] double minPrice = 0, [FromQuery] double maxPrice = 10000)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);
            return Ok(await courseService.GetCoursesPagenation(page, pageSize, minPrice, maxPrice));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CourseResponseDto>> GetCourseById(int id)
        {
            return Ok(await courseService.GetCourseById(id));
        }

        [HttpGet("{courseId:int}/students")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<List<StudentResponseDto>>> GetPaidStudents(int courseId)
        {
            return Ok(await courseService.GetPaidStudents(courseId));
        }

        // for task 7 
        [HttpGet("Count")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> CountCourses()
        {
            return Ok(await courseService.CountCourses());
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<CourseResponseDto>> CreateCourse(CreateCourseDto dto)
        {
            var createdCourse = await courseService.CreateCourse(dto);
            return CreatedAtAction(nameof(GetCourseById), new { id = createdCourse.Id }, createdCourse);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<CourseResponseDto>> UpdateCourse(int id , UpdateCourseDto updatedCourse) {
           
            var course = await courseService.GetCourseById(id);
            return Ok(await courseService.UpdateCourse(id, updatedCourse));   
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<Course>> DeleteCourse(int id)
        {
            return Ok(await courseService.DeleteCourse(id)); 
        }

        // Upload course image
        [HttpPost("upload-image")]
        [Authorize(Roles = UserRoles.Admin)]
        public async  Task<ActionResult> UploadImage(IFormFile file) {
            await using var stream = file.OpenReadStream();
            var imageUrl = await imageService.saveCourseImage(stream, file.FileName, file.Length);
            return Ok(new { image = imageUrl });    
        }
    }
}

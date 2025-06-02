using Final_Project_WebAPI.Data;
using Final_Project_WebAPI.DTO;
using Final_Project_WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<IEnumerable<CourseReadDTO>>> GetCourses()
        {
            var courses = await _context.Courses
                .Select(c => new CourseReadDTO
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    MediaUrl = c.MediaUrl
                })
                .ToListAsync();

            return courses;
        }

        // GET: api/Course/5
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<CourseReadDTO>> GetCourse(Guid id)
        {
            var c = await _context.Courses.FindAsync(id);

            if (c == null)
                return NotFound();

            var cours = new CourseReadDTO
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                MediaUrl = c.MediaUrl
            };

            return cours;
        }

        // PUT: api/Course/5
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> PutCourse(Guid id, CourseCreateDTO coursedto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            course.Title = coursedto.Title;
            course.Description = coursedto.Description;
            course.InstructorId = coursedto.InstructorId;
            course.MediaUrl = coursedto.MediaUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Course
        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<CourseReadDTO>> PostCourse(CourseCreateDTO coursedto)
        {
            var course = new Course
            {
                Title = coursedto.Title,
                Description = coursedto.Description,
                InstructorId = coursedto.InstructorId,
                MediaUrl = coursedto.MediaUrl,
                Assessments = []
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var dto = new CourseReadDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, dto);
        }

        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}

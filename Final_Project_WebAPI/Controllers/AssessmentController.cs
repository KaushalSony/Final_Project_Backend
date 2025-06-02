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
using System.Text.Json;
using System.Threading.Tasks;

namespace Final_Project_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssessmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssessmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Assessment
        [HttpGet]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<IEnumerable<AssessmentReadDTO>>> GetAssessments()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                var assessmentobj = await _context.Assessments.ToListAsync();
                var assessments = assessmentobj.Select(a =>
                {
                    List<QuestionDTO> questions;
                    if (!string.IsNullOrWhiteSpace(a.Questions) && a.Questions.TrimStart().StartsWith("["))
                    {
                        try
                        {
                            questions = JsonSerializer.Deserialize<List<QuestionDTO>>(a.Questions, options) ?? new List<QuestionDTO>();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Deserialization error for AssessmentId {a.AssessmentId}: {ex.Message}");
                            questions = new List<QuestionDTO>();
                        }
                    }
                    else
                    {
                        questions = new List<QuestionDTO>();
                    }

                    return new AssessmentReadDTO
                    {
                        AssessmentId = a.AssessmentId,
                        CourseId = a.CourseId,
                        Title = a.Title,
                        Questions = questions,
                        MaxScore = a.MaxScore
                    };
                }).ToList();

                return assessments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssessments: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Assessment/5
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<AssessmentReadDTO>> GetAssessment(Guid id)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var a = await _context.Assessments.FindAsync(id);

            if (a == null)
                return NotFound();

            List<QuestionDTO> questions;
            if (!string.IsNullOrWhiteSpace(a.Questions) && a.Questions.TrimStart().StartsWith("["))
            {
                try
                {
                    questions = JsonSerializer.Deserialize<List<QuestionDTO>>(a.Questions, options) ?? new List<QuestionDTO>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deserialization error for AssessmentId {a.AssessmentId}: {ex.Message}");
                    questions = new List<QuestionDTO>();
                }
            }
            else
            {
                questions = new List<QuestionDTO>();
            }

            var assessmt = new AssessmentReadDTO
            {
                AssessmentId = a.AssessmentId,
                CourseId = a.CourseId,
                Title = a.Title,
                Questions = questions,
                MaxScore = a.MaxScore
            };

            return assessmt;
        }

        // PUT: api/Assessment/5
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> PutAssessment(Guid id, AssessmentCreateDTO assessmentdto)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null)
                return NotFound();

            if (!ValidateQuestions(assessmentdto.Questions, out var error))
                return BadRequest(error);

            assessment.CourseId = assessmentdto.CourseId;
            assessment.Title = assessmentdto.Title;
            assessment.Questions = System.Text.Json.JsonSerializer.Serialize(assessmentdto.Questions);
            assessment.MaxScore = assessmentdto.MaxScore;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssessmentExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Assessment
        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<AssessmentReadDTO>> PostAssessment(AssessmentCreateDTO assessmentdto)
        {
            if (!ValidateQuestions(assessmentdto.Questions, out var error))
                return BadRequest(error);

            // Check if CourseId exists
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == assessmentdto.CourseId);
            if (!courseExists)
                return BadRequest("Invalid CourseId: Course does not exist.");

            var assessment = new Assessment
            {
                CourseId = assessmentdto.CourseId,
                Title = assessmentdto.Title,
                Questions = System.Text.Json.JsonSerializer.Serialize(assessmentdto.Questions),
                MaxScore = assessmentdto.MaxScore,
                Results = []
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            var dto = new AssessmentReadDTO
            {
                AssessmentId = assessment.AssessmentId,
                CourseId = assessment.CourseId,
                Title = assessment.Title,
                Questions = assessmentdto.Questions,
                MaxScore = assessment.MaxScore
            };

            return CreatedAtAction(nameof(GetAssessment), new { id = assessment.AssessmentId }, dto);
        }

        // DELETE: api/Assessment/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            var assessment = await _context.Assessments.FindAsync(id);
            if (assessment == null)
                return NotFound();

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssessmentExists(Guid id)
        {
            return _context.Assessments.Any(e => e.AssessmentId == id);
        }

        private bool ValidateQuestions(List<QuestionDTO> questions, out string? error)
        {
            if (questions == null || questions.Count == 0)
            {
                error = "Questions list cannot be empty.";
                return false;
            }
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                if (string.IsNullOrWhiteSpace(q.QuestionText))
                {
                    error = $"Question {i + 1} text is required.";
                    return false;
                }
                if (q.Options == null || q.Options.Count < 2 || q.Options.Count > 4)
                {
                    error = $"Question {i + 1} must have between 2 to 4 options.";
                    return false;
                }
                if (q.CorrectOption < 0 || q.CorrectOption >= q.Options.Count)
                {
                    error = $"Question {i + 1} has an invalid correct option index.";
                    return false;
                }
            }
            error = null;
            return true;
        }

        private bool ValidateAssessment(AssessmentCreateDTO dto, out string? error)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                error = "Title is required.";
                return false;
            }
            if (dto.MaxScore <= 0)
            {
                error = "MaxScore must be greater than zero.";
                return false;
            }
            // Optionally, check if CourseId exists in the database here
            return ValidateQuestions(dto.Questions, out error);
        }
    }
}

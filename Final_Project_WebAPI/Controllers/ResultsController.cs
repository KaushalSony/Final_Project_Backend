using EduSyncAPI.Services;
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
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Final_Project_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EventHubService _eventHubService; // Inject EventHubService

        public ResultsController(AppDbContext context, EventHubService eventHubService)
        {
            _context = context;
            _eventHubService = eventHubService;
        }

        // GET: api/Results
        [HttpGet]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<IEnumerable<ResultReadDTO>>> GetResults()
        {
            var results = await _context.Results
                .Select(r => new ResultReadDTO
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    UserId = r.UserId,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate
                })
                .ToListAsync();
            return Ok(results);
        }

        // GET: api/Results/5
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<ResultReadDTO>> GetResult(Guid id)
        {
            var r = await _context.Results.FindAsync(id);

            if (r == null)
                return NotFound();

            var dto = new ResultReadDTO
            {
                ResultId = r.ResultId,
                AssessmentId = r.AssessmentId,
                UserId = r.UserId,
                Score = r.Score,
                AttemptDate = r.AttemptDate
            };

            return Ok(dto);
        }

        // PUT: api/Results/5
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> PutResult(Guid id, ResultCreateDTO resultdto)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null)
                return NotFound();
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            result.Score = resultdto.Score;
            result.AttemptDate = resultdto.AttemptDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Results
        [HttpPost("assessment/{assessmentId}")]
        [Authorize(Policy = "RequireStudentRole")]
        public async Task<ActionResult<ResultReadDTO>> PostResult(Guid assessmentId, ResultCreateDTO resultdto)
        {
            // Get the userId from the JWT claims for security
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            var result = new Result
            {
                AssessmentId = assessmentId,
                UserId = Guid.Parse(userIdClaim),
                Score = resultdto.Score,
                AttemptDate = resultdto.AttemptDate
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            // Send event to Event Hub (using anonymous object, no new model needed)
            await _eventHubService.SendEventAsync(new
            {
                ResultId = result.ResultId,
                AssessmentId = assessmentId,
                UserId = userIdClaim,
                Score = resultdto.Score,
                AttemptDate = resultdto.AttemptDate,
                EventType = "QuizResultSubmitted",
                Timestamp = DateTime.UtcNow
            }, "QuizResultSubmitted");

            var dto = new ResultReadDTO
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                UserId = result.UserId,
                Score = result.Score,
                AttemptDate = result.AttemptDate
            };

            return CreatedAtAction(nameof(GetResult), new { id = result.ResultId }, dto);
        }

        // DELETE: api/Results/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null)
                return NotFound();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultExists(Guid id)
        {
            return _context.Results.Any(e => e.ResultId == id);
        }
    }
}

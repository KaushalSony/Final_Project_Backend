using Final_Project_WebAPI.Data;
using Final_Project_WebAPI.DTO;
using Final_Project_WebAPI.Models;
using Final_Project_WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Final_Project_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public UserController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/User
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetUsers()
        {
            var users = await _context.Users.Select(u => new UserReadDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                }).ToListAsync();
            return users;
        }

        [HttpGet("email")]
        [Authorize]
        public async Task<ActionResult<Guid>> GetUserIdByEmail()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("Email claim not found.");

            var user = await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new { u.UserId })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            return Ok(user.UserId);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<ActionResult<UserReadDTO>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("User Not Found");
            }

            return new UserReadDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        // GET: api/User/students
        [HttpGet("students")]
        [Authorize(Policy = "RequireInstructorRole")]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetStudents()
        {
            var students = await _context.Users
                .Where(u => u.Role == "Student")
                .Select(u => new UserReadDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            if (students.Count == 0)
            {
                return BadRequest("No students found.");
            }

            return students;
        }

        [HttpGet("student/{id}")]
        [Authorize(Policy = "RequireInstructorRole")]
        public async Task<ActionResult<UserReadDTO>> GetStudentById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is not { Role: "Student" })
            {
                return NotFound("Student not found.");
            }

            return new UserReadDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorOrStudentRole")]
        public async Task<IActionResult> PutUser(Guid id, UserUpdateDTO userdto)
        {
            var userobj = await _context.Users.FindAsync(id);
            if (userobj == null)
            {
                return NotFound("User Not Found");
            }

            var curusr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!(User.IsInRole("Admin") || userobj.UserId.ToString() == curusr))
                return Forbid();

            userobj.Name = userdto.Name;
            userobj.Email = userdto.Email;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound("User Not Found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<UserReadDTO>> PostUser(UserCreateDTO userdto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userdto.Email))
                return BadRequest("Email already exists");

            if (!Final_Project_WebAPI.Models.User.AllowedRoles.Contains(userdto.Role))
            {
                throw new ArgumentException("Invalid role. Allowed roles are: Instructor, Student.");
            }

            User userobj = new User();
            userobj.Name = userdto.Name;
            userobj.Email = userdto.Email;
            userobj.Role = userdto.Role;
            userobj.PasswordHash = BCrypt.Net.BCrypt.HashPassword("ABCD@1234");
            userobj.Courses = [];
            userobj.Results = [];

            _context.Users.Add(userobj);
            await _context.SaveChangesAsync();

            // Send welcome email with error handling
            try
            {
                await _emailService.SendEmailAsync(
                    userobj.Email,
                    "Welcome to the Platform",
                    $"Hello {userobj.Name},<br/>Your account has been created successfully. Your default Password is ABCD@1234. So change it to a rememberable password according to you."
                );
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your logger if available)
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
            }

            var usrdtl = new UserReadDTO
            {
                UserId = userobj.UserId,
                Name = userobj.Name,
                Email = userobj.Email,
                Role = userobj.Role
            };

            return CreatedAtAction("GetUser", new { id = userobj.UserId }, usrdtl);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}

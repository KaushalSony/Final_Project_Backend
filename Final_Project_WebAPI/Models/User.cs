using System;
using System.Collections.Generic;

namespace Final_Project_WebAPI.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public static readonly string[] AllowedRoles = { "Instructor", "Student" };
}

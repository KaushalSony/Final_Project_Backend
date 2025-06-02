using System;
using System.Collections.Generic;

namespace Final_Project_WebAPI.Models;

public partial class Result
{
    public Guid ResultId { get; set; }

    public Guid AssessmentId { get; set; }

    public Guid UserId { get; set; }

    public int Score { get; set; }

    public DateTime AttemptDate { get; set; }

    public virtual Assessment Assessment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

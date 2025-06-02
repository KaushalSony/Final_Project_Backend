using System.Collections.Generic;

namespace Final_Project_WebAPI.DTO
{
    public class QuestionDTO
    {
        public string QuestionText { get; set; } = null!;
        public List<string> Options { get; set; } = new();
        public int CorrectOption { get; set; } // Index of the correct option
        public int Score { get; set; } // Points for this question
    }
}
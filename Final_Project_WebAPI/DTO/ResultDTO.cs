namespace Final_Project_WebAPI.DTO
{
    public class ResultReadDTO
    {
        public Guid ResultId { get; set; }
        public Guid AssessmentId { get; set; }

        public Guid UserId { get; set; }

        public int Score { get; set; }

        public DateTime AttemptDate { get; set; }

    }
    public class ResultCreateDTO
    {
        //public Guid AssessmentId { get; set; }

        //public Guid UserId { get; set; }

        public int Score { get; set; }

        public DateTime AttemptDate { get; set; }

    }


}

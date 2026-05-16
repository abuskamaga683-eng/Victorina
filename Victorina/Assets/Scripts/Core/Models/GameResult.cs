namespace Victorina.Core.Models
{
    public class GameResult
    {
        public int Id { get; set; }

        public int SessionId { get; set; }

        public int TeamId { get; set; }

        public int? QuestionId { get; set; }

        public int Score { get; set; }
    }
}
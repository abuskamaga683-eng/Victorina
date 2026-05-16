using Victorina.Core.Interfaces;
using Victorina.Core.Models;

namespace Victorina.Data
{
    public class GameResultRepository : IGameResultRepository
    {
        private readonly IDatabaseService _db;

        public GameResultRepository(IDatabaseService db)
        {
            _db = db;
        }

        public void Add(GameResult result)
        {
            _db.ExecuteNonQuery(
                @"INSERT INTO GameResults 
                (SessionId, TeamId, Score)
                VALUES (@sessionId, @teamId, @score)",

                ("@sessionId", result.SessionId),
                ("@teamId", result.TeamId),
                ("@score", result.Score));
        }
    }
}
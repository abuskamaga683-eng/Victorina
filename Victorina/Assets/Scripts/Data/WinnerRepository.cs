using System.Collections.Generic;
using System.Linq;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;

namespace Victorina.Data
{
    public class WinnerRepository
    {
        private readonly IDatabaseService _db;

        public WinnerRepository(IDatabaseService db)
        {
            _db = db;
        }

        public List<WinnerInfo> GetAll()
        {
            string sql = @"
SELECT 
    gs.Id             AS SessionId,
    gs.Date           AS GameDate,
    t.Name            AS TeamName,
    gr.Score          AS TeamScore
FROM GameSessions gs
JOIN GameResults gr ON gr.SessionId = gs.Id
JOIN Teams t        ON t.Id = gr.TeamId
ORDER BY gs.Id DESC, gr.Score DESC
";

            var rows = _db.ExecuteQuery(sql, r => new
            {
                SessionId = r.GetInt32(0),
                Date = r.GetString(1),
                TeamName = r.GetString(2),
                Score = r.GetInt32(3)
            });

            var sessions = new Dictionary<int, WinnerInfo>();

            foreach (var row in rows)
            {
                if (!sessions.ContainsKey(row.SessionId))
                {
                    sessions[row.SessionId] = new WinnerInfo
                    {
                        Date = row.Date
                    };
                }

                sessions[row.SessionId]
                    .Teams
                    .Add($"{row.TeamName} — {row.Score}");
            }

            foreach (var session in sessions.Values)
            {
                int bestScore = -1;
                string winner = "";

                foreach (var team in session.Teams)
                {
                    var parts = team.Split('—');

                    if (parts.Length < 2)
                        continue;

                    string teamName = parts[0].Trim();

                    int score = int.Parse(parts[1].Trim());

                    if (score > bestScore)
                    {
                        bestScore = score;
                        winner = teamName;
                    }
                }

                session.WinnerName = winner;
            }

            return sessions.Values.ToList();
        }
    }
}
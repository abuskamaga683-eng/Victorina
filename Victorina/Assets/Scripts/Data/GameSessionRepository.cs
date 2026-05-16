using Mono.Data.Sqlite;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;

namespace Victorina.Data
{
    public class GameSessionRepository : IGameSessionRepository
    {
        private readonly string _connectionString;

        public GameSessionRepository(IDatabaseService db)
        {
            var databaseService = (DatabaseService)db;

            var field = typeof(DatabaseService)
                .GetField("_connectionString",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            _connectionString = (string)field.GetValue(databaseService);
        }

        public int Create(GameSession session)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText =
                @"INSERT INTO GameSessions (Date)
                  VALUES (@date);";

            cmd.Parameters.AddWithValue("@date", session.Date);

            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";

            return System.Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
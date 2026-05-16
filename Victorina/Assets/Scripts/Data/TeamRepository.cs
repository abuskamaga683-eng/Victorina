using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;

namespace Victorina.Data
{
    public class TeamRepository : ITeamRepository
    {
        private readonly string _connectionString;

        public TeamRepository(IDatabaseService db)
        {
            var databaseService = (DatabaseService)db;

            var field = typeof(DatabaseService)
                .GetField("_connectionString",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            _connectionString = (string)field.GetValue(databaseService);
        }

        public int Add(Team team)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText =
                @"INSERT INTO Teams (Name)
                  VALUES (@name);";

            cmd.Parameters.AddWithValue("@name", team.Name);

            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";

            return System.Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Team> GetAll()
        {
            return new List<Team>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;
using Victorina.Core.Interfaces;

namespace Victorina.Data
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            string path;
            if (Application.isEditor)
            {
                string dataFolder = System.IO.Path.Combine(Application.dataPath, "Data");
                System.IO.Directory.CreateDirectory(dataFolder);
                path = System.IO.Path.Combine(dataFolder, "victorina.db");
            }
            else
            {
                path = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Application.dataPath), "victorina.db");
            }
            _connectionString = $"URI=file:{path}";
            Debug.Log($"[DatabaseService] Путь к БД: {path}");
        }

        public void Initialize()
        {
            Debug.Log("[DatabaseService] Initialize — создание таблиц");

            try
            {
                ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS questions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                pool_index INTEGER NOT NULL DEFAULT 0,
                text TEXT NOT NULL,
                answer TEXT DEFAULT ''
            );
        ");
                ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS Teams (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL
            );
        ");

                ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS GameSessions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL
            );
        ");

                ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS GameResults (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SessionId INTEGER NOT NULL,
                TeamId INTEGER NOT NULL,
                QuestionId INTEGER,
                Score INTEGER NOT NULL DEFAULT 0,

                FOREIGN KEY (SessionId) REFERENCES GameSessions(Id) ON DELETE CASCADE,
                FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE,
                FOREIGN KEY (QuestionId) REFERENCES questions(id)
            );
        ");

                Debug.Log("[DatabaseService] ✓ Таблица questions создана / существует");
                Debug.Log("[DatabaseService] ✓ Таблица Teams создана / существует");
                Debug.Log("[DatabaseService] ✓ Таблица GameSessions создана / существует");
                Debug.Log("[DatabaseService] ✓ Таблица GameResults создана / существует");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DatabaseService] ОШИБКА CREATE TABLE: {e}");
                throw;
            }

            TryMigrate("ALTER TABLE questions ADD COLUMN pool_index INTEGER NOT NULL DEFAULT -1");
            TryMigrate("ALTER TABLE GameResults ADD COLUMN Score INTEGER NOT NULL DEFAULT 0");
        }

        private void TryMigrate(string sql)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                Debug.Log($"[DatabaseService] ✓ Миграция: {sql}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DatabaseService] Миграция пропущена: {e.Message}");
            }
        }

        public void ExecuteNonQuery(string sql, params (string name, object value)[] parameters)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                foreach (var (name, value) in parameters)
                    cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.LogError($"[DatabaseService] ОШИБКА ExecuteNonQuery: {e.Message}\nSQL: {sql}");
                throw;
            }
        }

        public List<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> mapper,
            params (string name, object value)[] parameters)
        {
            try
            {
                Debug.Log($"[DatabaseService] ExecuteQuery SQL: {sql}");
                if (parameters?.Length > 0)
                    Debug.Log($"[DatabaseService] Параметры: {string.Join(", ", parameters)}");
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                foreach (var (name, value) in parameters)
                    cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
                using var reader = cmd.ExecuteReader();
                var results = new List<T>();
                while (reader.Read())
                    results.Add(mapper(reader));
                Debug.Log($"[DatabaseService] ExecuteQuery вернул {results.Count} строк");
                return results;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DatabaseService] ОШИБКА ExecuteQuery: {e.Message}\nSQL: {sql}");
                throw;
            }
        }

        public long ExecuteScalar(string sql, params (string name, object value)[] parameters)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                foreach (var (name, value) in parameters)
                    cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
                var result = cmd.ExecuteScalar();
                long val = result is long l ? l : Convert.ToInt64(result);
                Debug.Log($"[DatabaseService] ExecuteScalar = {val}");
                return val;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DatabaseService] ОШИБКА ExecuteScalar: {e.Message}\nSQL: {sql}");
                throw;
            }
        }
    }
}

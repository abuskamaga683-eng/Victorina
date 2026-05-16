using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;

namespace Victorina.Data
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IDatabaseService _db;

        public QuestionRepository(IDatabaseService db) => _db = db;

        public List<Question> GetAll()
        {
            Debug.Log("[QuestionRepository] GetAll — запрос всех вопросов");
            var result = _db.ExecuteQuery(
                "SELECT id, pool_index, text, answer FROM questions ORDER BY id",
                Map);
            Debug.Log($"[QuestionRepository] GetAll вернул {result?.Count ?? 0} вопросов");
            return result;
        }

        public List<Question> GetByPool(int poolIndex)
        {
            Debug.Log($"[QuestionRepository] GetByPool({poolIndex}) — запрос вопросов из пула");
            var result = _db.ExecuteQuery(
                "SELECT id, pool_index, text, answer FROM questions WHERE pool_index = @pid ORDER BY id",
                Map,
                ("@pid", poolIndex));
            Debug.Log($"[QuestionRepository] GetByPool({poolIndex}) вернул {result?.Count ?? 0} вопросов");
            return result;
        }

        public Question GetById(int id)
        {
            var results = _db.ExecuteQuery(
                "SELECT id, pool_index, text, answer FROM questions WHERE id = @id",
                Map,
                ("@id", id));
            return results.Count > 0 ? results[0] : null;
        }

        public int Add(Question question)
        {
            _db.ExecuteNonQuery(
                "INSERT INTO questions (pool_index, text, answer) VALUES (@pid, @text, @ans)",
                ("@pid",  question.PoolIndex),
                ("@text", question.Text),
                ("@ans",  question.Answer ?? ""));
            return (int)_db.ExecuteScalar("SELECT last_insert_rowid()");
        }

        public void Update(Question question) =>
            _db.ExecuteNonQuery(
                "UPDATE questions SET pool_index = @pid, text = @text, answer = @ans WHERE id = @id",
                ("@pid",  question.PoolIndex),
                ("@text", question.Text),
                ("@ans",  question.Answer ?? ""),
                ("@id",   question.Id));

        public void Delete(int id) =>
            _db.ExecuteNonQuery("DELETE FROM questions WHERE id = @id", ("@id", id));

        private static Question Map(IDataRecord r) => new Question
        {
            Id        = r.GetInt32(0),
            PoolIndex = r.GetInt32(1),
            Text      = r.GetString(2),
            Answer    = r.IsDBNull(3) ? "" : r.GetString(3)
        };
    }
}

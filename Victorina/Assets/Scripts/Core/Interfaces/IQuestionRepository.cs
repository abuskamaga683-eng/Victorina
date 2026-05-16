using System.Collections.Generic;
using Victorina.Core.Models;

namespace Victorina.Core.Interfaces
{
    public interface IQuestionRepository
    {
        List<Question> GetAll();
        List<Question> GetByPool(int poolIndex);
        Question GetById(int id);
        int  Add(Question question);
        void Update(Question question);
        void Delete(int id);
    }
}

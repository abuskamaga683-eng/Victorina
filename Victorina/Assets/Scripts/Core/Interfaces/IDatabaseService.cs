using System;
using System.Collections.Generic;
using System.Data;

namespace Victorina.Core.Interfaces
{
    public interface IDatabaseService
    {
        void Initialize();
        void ExecuteNonQuery(string sql, params (string name, object value)[] parameters);
        List<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> mapper, params (string name, object value)[] parameters);
        long ExecuteScalar(string sql, params (string name, object value)[] parameters);
    }
}

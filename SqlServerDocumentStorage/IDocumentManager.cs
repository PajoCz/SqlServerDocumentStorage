using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlServerDocumentStorage
{
    public interface IDocumentManager : IDisposable
    {
        Task SaveChangesAsync();
        InsertionContext<T> Add<T>(T document);
        Task<T> GetAsync<T>(long id) where T : new();
        Task<List<T>> FindWhereAsync<T>(string where, object parameters) where T : new();
        Task DeleteWhereAsync<T>(string where, object parameters);
        UpdateContext<T> Update<T>(long id, T document);
    }
}
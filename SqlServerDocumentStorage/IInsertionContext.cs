using System.Collections.Generic;

namespace SqlServerDocumentStorage
{
    public interface IInsertionContext : IStatemetContext
    {
        SqlContext Sql { get; }
        List<Index> Indices { get; }
        object Value { get; }
    }
}
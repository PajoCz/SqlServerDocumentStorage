using System.Collections.Generic;

namespace SqlServerDocumentStorage
{
    public interface IStatemetContext
    {
        SqlContext Sql { get; }
        List<Index> Indices { get; }
        object Value { get; }
    }
}
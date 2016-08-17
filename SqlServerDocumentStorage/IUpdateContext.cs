using System.Collections.Generic;

namespace SqlServerDocumentStorage
{
    public interface IUpdateContext : IStatemetContext
    {
        object Model { get; }
        SqlContext Sql { get; }
        List<Index> Indices { get; }
        object Value { get; }
    }

    public interface IUpdateContext<T> : IUpdateContext
    {
        T Model { get; }
        SqlContext Sql { get; }
        List<Index> Indices { get; }
        object Value { get; }
    }
}
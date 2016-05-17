using System.Collections.Generic;
using NUnit.Framework;

namespace SqlServerDocumentStorage
{
    public interface IInsertionContext
    {
        SqlContext Sql { get; }
        List<Index> Indices { get; }
        object Value { get; }
    }
}
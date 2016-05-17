using System.Collections.Generic;
using NUnit.Framework;

namespace SqlServerDocumentStorage
{
    public class InsertionContext<T> : IInsertionContext 
    {
        public InsertionContext(T model)
        {
            Model = model;
        }
        public T Model { get; }
        public SqlContext Sql { get; set; } = new SqlContext();
        public List<Index> Indices { get; set; } = new List<Index>();
        public object Value => Model;
    }
}
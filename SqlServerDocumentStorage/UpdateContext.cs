using System;
using System.Collections.Generic;

namespace SqlServerDocumentStorage
{
    public class UpdateContext<T> : IUpdateContext<T>
    {
        public UpdateContext(object document)
        {
            throw new NotImplementedException();
        }

        public UpdateContext(T model)
        {
            Model = model;
        }

        public T Model { get; }

        object IUpdateContext.Model
        {
            get { return Model; }
        }

        public SqlContext Sql { get; set; } = new SqlContext();
        public List<Index> Indices { get; set; } = new List<Index>();
        public object Value => Model;
    }
}
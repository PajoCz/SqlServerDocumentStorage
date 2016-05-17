using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace SqlServerDocumentStorage
{
    public class SqlContext
    {
        public string CommandText { get; set; }
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public ICollection<IDbDataParameter> Parameters { get; } = new Collection<IDbDataParameter>();

        public string GetFullTableName()
        {
            var name = $"[{SchemaName}].[{TableName}]";
            return name;
        }
    }
}
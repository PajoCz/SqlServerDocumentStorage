using System.Data;
using System.Reflection;

namespace SqlServerDocumentStorage
{
    public class Index
    {
        public DbType DbType { get; set; }
        public string Name { get; set; }
        public string Length { get; set; }
        public PropertyInfo Property { get; set; }
    }
}
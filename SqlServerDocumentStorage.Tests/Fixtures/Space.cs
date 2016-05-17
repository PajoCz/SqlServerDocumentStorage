using System.Data;

namespace SqlServerDocumentStorage.Tests.Fixtures
{
    public class Space
    {
        public long Id { get; set; }

        [Index(DbType.String, length:"200")]
        public string Name { get; set; }

        [Index(DbType.String, length:"max")]
        public string Description { get; set; }
    }
}
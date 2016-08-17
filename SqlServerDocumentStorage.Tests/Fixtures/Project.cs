using System.Collections.Generic;
using System.Data;

namespace SqlServerDocumentStorage.Tests.Fixtures
{
    public class Project
    {
        public long Id { get; set; }

        [Index(DbType.Int64)]
        public long SpaceId { get; set; }

        [Index(DbType.StringFixedLength, "200")]
        public string Title { get; set; }

        public List<Resource> Resources { get; set; }
    }
}
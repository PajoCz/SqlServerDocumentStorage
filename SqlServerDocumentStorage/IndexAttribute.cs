using System;
using System.Data;

namespace SqlServerDocumentStorage
{
    public class IndexAttribute : Attribute
    {
        public DbType Type { get; }
        public string Length { get; }
        public int Scale { get; }
        public int Precision { get; }

        public IndexAttribute(DbType type, string length = "", int scale=0, int precision=0)
        {
            Type = type;
            Length = length;
            Scale = scale;
            Precision = precision;
        }
    }
}
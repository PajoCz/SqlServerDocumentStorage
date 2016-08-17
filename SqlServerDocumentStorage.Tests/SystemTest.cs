using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlServerDocumentStorage.Tests
{
    public class SystemTest : IDisposable
    {
        private readonly SqlConnection connection;
        private readonly SqlTransaction transaction;

        private SystemTest(SqlConnection connection, SqlTransaction transaction)
        {
            this.connection = connection;
            this.transaction = transaction;
        }


        public void Dispose()
        {
            transaction?.Rollback();
            transaction?.Dispose();
            connection?.Close();
            connection?.Dispose();
        }

        public static async Task<SystemTest> CreateAsync()
        {
            var connectionString =
                @"Data Source=.\SQLEXPRESS;Initial Catalog=SqlServerDocumentStorage;Integrated Security=True";
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            var transaction = connection.BeginTransaction();
            return new SystemTest(connection, transaction);
        }

        public DocumentManager GetDocumentManager()
        {
            return new DocumentManager(connection, transaction);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SqlServerDocumentStorage
{
    public class DocumentManager : IDisposable
    {
        private readonly SqlConnection connection;
        private readonly SqlTransaction transaction;
        private readonly ICollection<IInsertionContext> additions = new Collection<IInsertionContext>();

        public DocumentManager(SqlConnection connection, SqlTransaction transaction)
        {
            this.connection = connection;
            this.transaction = transaction;
        }

        public bool AutoCreateTables { get; set; }

        public async Task SaveChangesAsync()
        {
            foreach (var add in additions)
            {
                try
                {
                    await InsertAsync(add);

                }
                catch (SqlException e)
                {
                    if (e.Message.Contains("Invalid object name ") || e.Message.Contains("Could not find"))
                    {
                        await CreateTableAsync(add);
                        await InsertAsync(add);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        private async Task InsertAsync(IInsertionContext add)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = add.Sql.CommandText;
                command.CommandType = CommandType.StoredProcedure;
                foreach (var parameter in add.Sql.Parameters)
                {
                    command.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
                }

                foreach (var index in add.Indices)
                {
                    command.Parameters.AddWithValue(index.Name, index.Property.GetValue(add.Value));
                }

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var id = reader.GetFieldValue<long>(reader.GetOrdinal("Id"));
                        var valueType = add.Value.GetType();
                        var idProperty = valueType.GetProperty("Id");
                        idProperty.SetValue(add.Value, id);
                    }
                }
            }
        }

        private async Task CreateTableAsync(IInsertionContext add)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("IF NOT EXISTS (");
            builder.AppendLine("SELECT  schema_name ");
            builder.AppendLine("FROM    information_schema.schemata ");
            builder.AppendLine("WHERE   schema_name = '" + add.Sql.SchemaName + "')");

            builder.AppendLine("BEGIN");
            builder.AppendLine("EXEC sp_executesql N'CREATE SCHEMA [" + add.Sql.SchemaName + "]'");
            builder.AppendLine("END");

            builder.Append("create table ");
            builder.Append(add.Sql.GetFullTableName());
            builder.Append("(");
            builder.Append("Id bigint identity(1,1) primary key not null,");

            foreach (var index in add.Indices)
            {
                builder.Append(index.Name + " " + GetDllColumnDefinition(index) + " not null,");
            }

            builder.Append("Data nvarchar(max) not null,");
            builder.Append("CreatedAt datetime2 not null");
            builder.Append(");");

            var sprocBuilder = new StringBuilder();
            sprocBuilder.AppendLine("create procedure [" + add.Sql.SchemaName + "].[" + add.Sql.TableName + "_Insert]");
            sprocBuilder.AppendLine("(");
            foreach (var index in add.Indices)
            {
                sprocBuilder.Append("   @" + index.Name + " " + GetDllColumnDefinition(index) + ",");
            }
            sprocBuilder.AppendLine("    @Data nvarchar(max),");
            sprocBuilder.AppendLine("    @CreatedAt datetime2");
            sprocBuilder.AppendLine(")");
            sprocBuilder.AppendLine("as");
            sprocBuilder.AppendLine("insert into " + add.Sql.GetFullTableName());
            sprocBuilder.AppendLine("(");
            foreach (var index in add.Indices)
            {
                sprocBuilder.Append("   " + index.Name + ",");
            }
            sprocBuilder.AppendLine("    Data,");
            sprocBuilder.AppendLine("    CreatedAt");
            sprocBuilder.AppendLine(")");
            sprocBuilder.AppendLine("output inserted.Id");
            sprocBuilder.AppendLine("values");
            sprocBuilder.AppendLine("(");
            foreach (var index in add.Indices)
            {
                sprocBuilder.Append("    @" + index.Name + ",");
            }
            sprocBuilder.AppendLine("    @Data,");
            sprocBuilder.AppendLine("    @CreatedAt");
            sprocBuilder.AppendLine(")");

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = builder.ToString();
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                command.CommandText = sprocBuilder.ToString();
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private object GetDllColumnDefinition(Index index)
        {
            switch (index.DbType)
            {
                case DbType.String:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                    return "nvarchar(" + index.Length + ")";
                case DbType.Binary:
                    break;
                case DbType.Byte:
                    break;
                case DbType.Boolean:
                    break;
                case DbType.Currency:
                    break;
                case DbType.Date:
                    break;
                case DbType.DateTime:
                    break;
                case DbType.Decimal:
                    break;
                case DbType.Double:
                    break;
                case DbType.Guid:
                    break;
                case DbType.Int16:
                    return "smallint";
                case DbType.Int32:
                    return "int";
                case DbType.Int64:
                    return "bigint";
                case DbType.Object:
                    break;
                case DbType.SByte:
                    break;
                case DbType.Single:
                    break;
                case DbType.Time:
                    break;
                case DbType.UInt16:
                    break;
                case DbType.UInt32:
                    break;
                case DbType.UInt64:
                    break;
                case DbType.VarNumeric:
                    break;
                case DbType.Xml:
                    break;
                case DbType.DateTime2:
                    break;
                case DbType.DateTimeOffset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new ArgumentOutOfRangeException();
        }

        public InsertionContext<T> Add<T>(T document)
        {
            var schemaName = "Documents";
            var tableName = typeof(T).Name;

            var context = new InsertionContext<T>(document);
            context.Sql.TableName = tableName;
            context.Sql.SchemaName = schemaName;
            context.Sql.CommandText = "[" + context.Sql.SchemaName + "].[" + context.Sql.TableName + "_Insert]";

            var indices = GetIndices<T>();
            context.Indices.AddRange(indices);


            string data;

            var serializerSettings = GetJsonSerializerSettings(context);
            data = JsonConvert.SerializeObject(context.Model, serializerSettings);

            context.Sql.Parameters.Add(new SqlParameter() { ParameterName = "Data", Value = data });
            context.Sql.Parameters.Add(new SqlParameter() { ParameterName = "CreatedAt", Value = DateTime.Now });

            additions.Add(context);
            return context;
        }

        private static JsonSerializerSettings GetJsonSerializerSettings<T>(InsertionContext<T> context)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new MyContractResolver<T>(context);
            return serializerSettings;
        }

        private static List<Index> GetIndices<T>()
        {
            List<Index> indices = new List<Index>();
            var type = typeof (T);
            var instanceProperties = type.GetProperties();
            var indexProperties = instanceProperties.Where(x => x.GetCustomAttribute<IndexAttribute>() != null).ToArray();

            foreach (var indexProperty in indexProperties)
            {
                var indexAttribute = indexProperty.GetCustomAttribute<IndexAttribute>();
                var index = new Index();
                index.DbType = indexAttribute.Type;
                index.Name = indexProperty.Name;
                index.Length = indexAttribute.Length;
                index.Property = indexProperty;
                indices.Add(index);
            }

            return indices;
        }

        public void Dispose()
        {
        }

        public async Task<T> GetAsync<T>(long id) where T : new()
        {
            var schemaName = "Documents";
            var tableName = typeof(T).Name;

            var builder = new StringBuilder()
                .Append("select top 1 * from ")
                .Append("[")
                .Append(schemaName)
                .Append("].[")
                .Append(tableName)
                .Append("] where Id = @Id");

            T instance = default(T);

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = builder.ToString();
                command.Parameters.AddWithValue("Id", id);
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var json = reader.GetFieldValue<string>(reader.GetOrdinal("Data"));
                        instance = JsonConvert.DeserializeObject<T>(json);
                        id = reader.GetFieldValue<long>(reader.GetOrdinal("Id"));
                        SetPropertyValue(instance, "Id", id);
                        var indices = GetIndices<T>();
                        foreach (var index in indices)
                        {
                            var indexColumValue = reader.GetValue(reader.GetOrdinal(index.Name));
                            SetPropertyValue(instance, index.Name, indexColumValue);
                        }
                    }
                }
            }

            return instance;
        }

        private static void SetPropertyValue<T>(T instance, string propertyName, object value)
        {
            var type = instance.GetType();
            var property = type.GetProperty(propertyName);
            property.SetValue(instance, value);
        }

        public async Task<List<T>>  FindWhereAsync<T>(string where, object parameters) where T : new()
        {
            var schemaName = "Documents";
            var tableName = typeof(T).Name;

            var builder = new StringBuilder()
                .Append("select top 1 * from ")
                .Append("[")
                .Append(schemaName)
                .Append("].[")
                .Append(tableName)
                .Append("] where ")
                .Append(where);

            List<T> list = new List<T>();

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = builder.ToString();

                var sqlParameters = parameters.GetType().GetProperties().Select(x =>
                    new SqlParameter(x.Name, x.GetValue(parameters)));

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        T instance = default(T);
                        var json = reader.GetFieldValue<string>(reader.GetOrdinal("Data"));
                        instance = JsonConvert.DeserializeObject<T>(json);
                        var id = reader.GetFieldValue<long>(reader.GetOrdinal("Id"));
                        SetPropertyValue(instance, "Id", id);
                        var indices = GetIndices<T>();
                        foreach (var index in indices)
                        {
                            var indexColumValue = reader.GetValue(reader.GetOrdinal(index.Name));
                            SetPropertyValue(instance, index.Name, indexColumValue);
                        }

                        list.Add(instance);
                    }
                }
            }

            return list;
        }
    }
}
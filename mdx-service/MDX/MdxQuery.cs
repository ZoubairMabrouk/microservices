using Microsoft.AnalysisServices.AdomdClient;
using System.Data;

namespace MdxServices.MDX
{
    public class MdxQuery : IMdxQuery
    {
        private readonly string _connectionString;
        private readonly ILogger<MdxQuery> _logger;

        public MdxQuery(IConfiguration configuration, ILogger<MdxQuery> logger)
        {
            _connectionString = configuration.GetConnectionString("MDConnection")
                ?? throw new InvalidOperationException("MDConnection string is not configured.");
            _logger = logger;
        }

        public DataTable ExecuteMdxQuery(string mdxQuery)
        {
            _logger.LogDebug("Executing MDX: {Query}", mdxQuery);

            using var connection = new AdomdConnection(_connectionString);
            connection.Open();

            using var command = new AdomdCommand(mdxQuery, connection)
            {
                CommandTimeout = 300
            };

            using var reader = command.ExecuteReader();

            var dataTable = new DataTable();
            dataTable.Constraints.Clear();

            for (int i = 0; i < reader.FieldCount; i++)
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

            while (reader.Read())
            {
                var row = dataTable.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[i] = reader[i] == DBNull.Value ? null! : reader[i];
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
using MdxServices.Interfaces;
using MdxServices.MDX;
using System.Data;

namespace MdxServices.Services
{
    public class MdxService : IMdxService
    {
        private readonly IMdxQuery _mdxQuery;
        private readonly ILogger<MdxService> _logger;

        public MdxService(IMdxQuery mdxQuery, ILogger<MdxService> logger)
        {
            _mdxQuery = mdxQuery;
            _logger = logger;
        }

        public IEnumerable<Dictionary<string, object?>> Execute(string mdxQuery)
        {
            _logger.LogInformation("Executing MDX query");
            DataTable table = _mdxQuery.ExecuteMdxQuery(mdxQuery);
            return MapTableToJson(table);
        }

        private static IEnumerable<Dictionary<string, object?>> MapTableToJson(DataTable table)
        {
            var result = new List<Dictionary<string, object?>>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object?>(table.Columns.Count);
                foreach (DataColumn col in table.Columns)
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                result.Add(dict);
            }
            return result;
        }
    }
}

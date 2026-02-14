using SQLiteDS_ChatGPT_2.Workers;
using System.Collections.Concurrent;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class ModelSqlFactory
    {
        private static readonly ConcurrentDictionary<WorkType, string> _cache = [];

        public static string GetInsertSql(WorkType type)
            => _cache.GetOrAdd(type, Build);
        private static string Build(WorkType type)
        {
            var attr = type.GetTableInfo();
            var props = ModelMetaCache
                .GetProps(attr.ModelType)
                .Where(p => p.Name != "Idx")
                .ToArray();
            var cols = string.Join(",",
                props.Select(p => $"[{p.Name}]"));
            var vals = string.Join(",",
                props.Select(p => $"@{p.Name}"));
            return $"INSERT INTO [{attr.TableName}] ({cols}) VALUES ({vals});";
        }
    }
}

using System.Reflection;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class ModelSqlFactory
    {
        public static string BuildInsertSql(Type model, string table)
        {
            var props = model.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.Name != "Idx")
                .ToArray();

            var cols = string.Join(",", props.Select(p => p.Name));
            var vals = string.Join(",", props.Select (p => $"@{p.Name}"));

            return $"INSERT INTO [{table}] ({cols}) VALUES ({vals});";
        }
    }
}

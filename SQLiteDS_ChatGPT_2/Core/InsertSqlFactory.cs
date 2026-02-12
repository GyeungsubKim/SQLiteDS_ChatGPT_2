using System.Reflection;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class InsertSqlFactory
    {
        public static string CreateInsertSql(
                string table,
                Type model)
        {
            var props = GetAllProperties(model);

            var cols = string.Join(",",
                props.Select(p => $"[{p.Name}]"));

            var vals = string.Join(",",
                props.Select((p, i) => $"@p{i}"));

            return $"INSERT INTO [{table}] ({cols}) VALUES ({vals});";
        }

        private static List<PropertyInfo> GetAllProperties(Type t)
        {
            var list = new List<PropertyInfo>();

            while (t != null && t != typeof(object))
            {
                list.AddRange(
                    t.GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.DeclaredOnly));

                t = t.BaseType!;
            }

            return list
                .GroupBy(p => p.Name)
                .Select(g => g.First())
                .ToList();
        }
    }
}

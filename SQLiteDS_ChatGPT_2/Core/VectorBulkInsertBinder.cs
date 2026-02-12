using Microsoft.Data.Sqlite;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLiteDS_ChatGPT_2.Core
{
    public class VectorBulkInsertBinder<T> 
    {
        private readonly Action<SqliteCommand, T> _binder;

        public VectorBulkInsertBinder(SqliteCommand cmd)
            => _binder = CompileBinder(cmd);
        public void Execute(SqliteCommand cmd, IEnumerable<T> models)
        {
            foreach (var m in models)
            {
                _binder(cmd, m);
                cmd.ExecuteNonQuery();
            }
        }
        private Action<SqliteCommand, T> CompileBinder(SqliteCommand cmd) 
        {
            var modelParam = Expression.Parameter(typeof(T));
            var cmdParam = Expression.Parameter(typeof(SqliteCommand));

            var body = new List<Expression>();

            int index = 0;

            var props = GetAllProperties(typeof(T));

            foreach (var p in props)
            {
                if (!p.CanWrite) continue;

                var valueExp = Expression.Property(modelParam, p);
                var paramExp = Expression.Property(
                    Expression.Property(cmdParam, "Parameters"),
                    "Item",
                    Expression.Constant(index));
                var assign =
                    Expression.Assign(
                        Expression.Property(paramExp, "Value"),
                        Expression.Convert(valueExp, typeof(object)));

                body.Add(assign);
                index++;
            }

            var block = Expression.Block(body);

            return Expression
                .Lambda<Action<SqliteCommand, T>>(
                    block,
                    cmdParam,
                    modelParam)
                .Compile();
        }
        private List<PropertyInfo> GetAllProperties(Type t)
        {
            var list = new List<PropertyInfo>();

            while(t != null && t != typeof(object))
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

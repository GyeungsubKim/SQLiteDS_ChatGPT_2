using Microsoft.Data.Sqlite;
using System.Linq.Expressions;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class ModelBinderCompiler
    {
        public static Action<SqliteCommand, object> Compile(Type type)
        {
            var cmdParam = Expression.Parameter(typeof(SqliteCommand), "cmd");
            var objParam = Expression.Parameter(typeof(object), "obj");
            var castObj = Expression.Convert(objParam, type);

            var body = new List<Expression>();

            foreach (var prop in type.GetProperties())
            {
                if (prop.Name == "Idx") continue;

                var paramIndexer = Expression.Property(
                    Expression.Property(cmdParam, nameof(SqliteCommand.Parameters)),
                    "Item",
                    Expression.Constant($"@{prop.Name}"));

                var value = Expression.Convert(
                    Expression.Property(castObj, prop),
                    typeof(object));

                var assign = Expression.Assign(
                    Expression.Property(paramIndexer, nameof(SqliteParameter.Value)),
                    value);

                body.Add(assign);
            }

            var block = Expression.Block(body);

            return Expression.Lambda<Action<SqliteCommand, object>>(
                block, cmdParam, objParam).Compile();
        }
    }
}

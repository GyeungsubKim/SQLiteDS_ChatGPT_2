using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class ModelBinderCompile
    {
        private static readonly ConcurrentDictionary<WorkType, 
            Action<SqliteCommand, Basic>> _cache = [];
        private static readonly ConcurrentDictionary<WorkType,
            int> _paramCount = [];

        public static Action<SqliteCommand, Basic> Get(WorkType type)
            => _cache.GetOrAdd(type, Compile);
        public static int GetParamCount(WorkType type)
            => _paramCount[type];
        private static Action<SqliteCommand, Basic> Compile(WorkType type)
        {
            var attr = type.GetTableInfo();
            var modelType = attr.ModelType;

            var props = GetAllProperties(modelType)
                .Where(p => p.Name != "Idx")
                .ToArray();

            _paramCount[type] = props.Length;

            var cmdParam = Expression.Parameter(typeof(SqliteCommand), "cmd");
            var modelParam = Expression.Parameter(typeof(Basic), "model");

            var modelCast =
                Expression.Convert(modelParam, modelType);

            var body = new List<Expression>();
            var parameters =
                Expression.Property(
                    cmdParam, 
                    ((PropertyInfo)
                    ((MemberExpression)
                    ((Expression<Func<SqliteCommand,
                        SqliteParameterCollection>>)
                        (c => c.Parameters)).Body)
                    .Member));

            var addMethod = 
                typeof(SqliteParameterCollection)
                .GetMethod("AddWithValue",
                    [typeof(string), typeof(object)])!;
            
            foreach ( var prop in props)
            {
                var raw =
                    Expression.Property(modelCast, prop);
                var value =
                    Expression.Condition(
                        Expression.Equal(
                            Expression.Convert(raw, typeof(object)),
                            Expression.Constant(null)),
                        Expression.Constant(DBNull.Value, typeof(object)),
                        Expression.Convert(raw, typeof(object)));
                var call =
                    Expression.Call(
                        parameters,
                        addMethod,
                        Expression.Constant($"@{prop.Name}"),
                        value);

                body.Add(call);
            }
            var block = Expression.Block(body);

            return Expression.Lambda<Action<SqliteCommand, Basic>>(
                block, cmdParam, modelParam).Compile();
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
            return [.. list
                .GroupBy(p => p.Name)
                .Select(g => g.First())];
        }
    }
    

    public sealed class Binder
    {
        public PropertyInfo[] Properties { get; }
        public Func<object, object?>[] Getters { get; }

        public Binder(List<PropertyInfo> props, List<Func<object, object?>> getters)
        {
            Properties = props.ToArray();
            Getters = getters.ToArray();
        }
        public int Count => Getters.Length;
        public object? GetValue(object model, int index)
            => Getters[index](model);
    }
}

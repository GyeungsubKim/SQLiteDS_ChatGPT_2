using System.Collections.Concurrent;
using System.Reflection;

namespace SQLiteDS_ChatGPT_2.Workers
{
    public static class ModelMetaCache
    {
        private static readonly ConcurrentDictionary<Type,
            List<PropertyInfo>> _cache = [];
        public static List<PropertyInfo> GetProps(Type t)
            => _cache.GetOrAdd(t, Build);
        private static List<PropertyInfo> Build(Type t)
        {
            var list = new List<PropertyInfo>();

            while (t != null && t != typeof(object))
            {
                list.InsertRange(0,
                    t.GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.DeclaredOnly));

                t = t.BaseType!;
            }
            return list;
        }
    }
}

using System.Linq.Expressions;
using System.Reflection;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class BinaryLayoutValidator
    {
        public static void ValidateAll()
        {
            foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
            {
                var field = typeof(WorkType).GetField(type.ToString());
                var attr  = field?.GetCustomAttribute<TableInfoAttribute>();

                if (attr == null) continue;

                ValidateModel(attr.ModelType);
            }
        }
        private static void ValidateModel(Type model)
        {
            Console.WriteLine($"[CHECK] {model.Name}");

            var props = GetAllProperties(model);

            CheckDecimal(props, model);
            CheckDuplicate(props, model);
            CheckBasicFields(props, model);
        }
        private static void CheckDecimal(
            List<PropertyInfo> props, Type model)
        {
            foreach(var p in props)
            {
                if (p.PropertyType == typeof(decimal))
                    throw new Exception($"decimal 금지: {model.Name}.{p.Name}");
            }
        }
        private static void CheckDuplicate(
            List<PropertyInfo> props, Type model)
        {
            var dup = props.GroupBy(p => p.Name)
                           .Where(g => g.Count() > 1);

            foreach (var d in dup)
                throw new Exception($"중복 필드: {model.Name}.{d.Key}");
        }
        private static void CheckBasicFields(
            List<PropertyInfo> props, Type model)
        {
            var names = props.Select(p => p.Name).ToList();

            if (!names.Contains("Idx"))
                throw new Exception($"{model.Name} Idx 없씀");
            if (!names.Contains("Seq"))
                throw new Exception($"{model.Name} Seq 없씀");
            if (!names.Contains("Code"))
                throw new Exception($"{model.Name} Code 없씀");
        }
        private static List<PropertyInfo> GetAllProperties(Type t)
        {
            var list = new List<PropertyInfo>();

            while(t !=null && t != typeof(object))
            {
                list.AddRange(t.GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly));

                t = t.BaseType!;
            }
            return list;
        }
    }
}

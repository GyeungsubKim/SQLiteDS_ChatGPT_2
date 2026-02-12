using System.Reflection;
using System.Text;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class SqlCreateGenerator
    {
        public static string CreateTablesSql()
        {
            var sb = new StringBuilder();

            foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
            {
                var field = typeof(WorkType).GetField(type.ToString());
                var attr = field?.GetCustomAttribute<TableInfoAttribute>();
                if (attr == null) continue;

                sb.AppendLine(GenerateCreate(attr.TableName, attr.ModelType));
                sb.AppendLine();
            }
                //sb.AppendLine($"CREATE TABLE IF NOT EXISTS [{attr.TableName}] (");
                //sb.AppendLine(" [Idx] INTEGER PRIMARY KEY,");

                //var props = attr.ModelType.GetProperties()
                //    .Where(p => p.Name != "Idx")
                //    .ToList();
                
                //foreach (var p in props)
                //{
                //    string sqlType = MapType(p.PropertyType);
                //    sb.AppendLine($"{p.Name} {sqlType},");
                //}

                //sb.Length -= 3; // 마지막 콤마 제거
                //sb.AppendLine("\n) WITHOUT ROWID;");

                //    if (tableName.Contains("Code"))
                //    {
                //        sb.AppendLine($"CREATE INDEX IF NOT EXISTS IDX_{tableName}_Code ON [{tableName}] (Code);");
                //    }

                //    if (!tableName.Equals("tblWork"))
                //    {
                //        sb.AppendLine($"CREATE TRIGGER IF NOT EXISTS trg_Log{tableName} ");
                //        sb.AppendLine($"AFTER INSERT ON [{tableName}] ");
                //        sb.AppendLine("BEGIN");
                //        sb.AppendLine($"    INSERT INTO tblWork (TableId, ForeignKey) ");
                //        sb.AppendLine($"    VALUES ({(int)type}, NEW.Idx);");
                //        sb.AppendLine("END;");
                //    }
                //    sb.AppendLine();
            //}

            //sb.AppendLine("CREATE UNIQUE INDEX IF NOT EXISTS idxWorkID ON [tblWork] (TableId, ForeignKey);");
            //sb.AppendLine("CREATE UNIQUE INDEX IF NOT EXISTS idxHighLow ON [tblHighLow] (Code, Date);");
            //sb.AppendLine();

            return sb.ToString();
        }
        private static string GenerateCreate(string table, Type model)
        {
            var props = GetAllProperties(model);

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS [{table}] (");

            foreach (var p in props)
            {
                if (!p.CanWrite) continue;

                var sqlType = MapType(p.PropertyType);
                sb.AppendLine($"[{p.Name}] {sqlType},");
            }

            sb.Length -= 3;
            sb.AppendLine("\n) WITHOUT ROWID;");

            return sb.ToString();
        }
        private static List<PropertyInfo> GetAllProperties(Type t)
        {
            var list = new List<PropertyInfo>();

            while( t !=null && t != typeof(object))
            {
                list.AddRange(t.GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly));

                t = t.BaseType!;
            }

            return list
                .GroupBy(p => p.Name)
                .Select(g => g.First())
                .OrderBy(p => p.Name == "Idx" ? 0 : 1)
                .ToList();
        }

        private static string MapType(Type t)
        {
            var code = Type.GetTypeCode(t);
            return code switch
            {
                TypeCode.String => "TEXT",
                TypeCode.Byte or
                TypeCode.Int32 or 
                TypeCode.Int64 or 
                TypeCode.Boolean or
                TypeCode.DateTime => "INTEGER",
                TypeCode.Double => "REAL",
                _ => "TEXT",
            };
        }
    }
}

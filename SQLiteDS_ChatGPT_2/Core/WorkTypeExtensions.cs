using System.Reflection;

namespace SQLiteDS_ChatGPT_2.Core
{
    public static class WorkTypeExtensions
    {
        private static readonly Dictionary<WorkType, (string TableName, string SelectSql, string InsertSql, List<string> Columns)> _cache = [];
        static WorkTypeExtensions()
        {
            foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
            {
                var field = typeof(WorkType).GetField(type.ToString());
                var attr = field?.GetCustomAttribute<TableInfoAttribute>();
                if (attr == null && field != null)
                {
                    var attrs = field.GetCustomAttributes(typeof(TableInfoAttribute), false);
                    attr = attrs.FirstOrDefault() as TableInfoAttribute;
                }
                // 모델 타입의 공개 인스턴스 속성 중 Idx를 제외한 속성 이름 수집
                var props = attr?.ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name != "Idx")
                    .Select(p => p.Name)
                    .ToList();
                string tableName = attr!.TableName;
                string cols = string.Join(", ", props!);
                // 기본 조회 SQL: Idx를 포함시키고 컬럼들을 나열
                string selectSql = $"SELECT Idx, {cols} FROM {tableName}";
                // INSERT SQL 구성: 각 속성에 대응하는 파라미터 사용
                string values = string.Join(", ", props!.Select(p => "@" + p));
                string insertSql = $"INSERT OR IGNORE INTO {tableName} ({cols}) VALUES ({values})";
                _cache[type] = (tableName, selectSql, insertSql, props!);
            }
        }

        // 확장 메서드: 각 WorkType에 대해 테이블명/SELECT/INSERT SQL 제공
        public static string GetTableName(this WorkType type) => _cache[type].TableName;
        public static string GetSelectSql(this WorkType type) => _cache[type].SelectSql;
        public static string GetInsertSql(this WorkType type) => _cache[type].InsertSql;
        public static List<string> GetColums(this WorkType type) => _cache[type].Columns;
    }
}

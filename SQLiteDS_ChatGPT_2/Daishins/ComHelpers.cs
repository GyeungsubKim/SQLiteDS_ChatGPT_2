namespace SQLiteDS_ChatGPT_2.Daishins
{
    internal static class ComHelpers
    {
        public static string AsString(object? obj) => obj?.ToString() ?? string.Empty;

        public static int AsLong(object? obj, int fallback = 0)
        {
            if (obj == null) return fallback;
            if (int.TryParse(obj.ToString(), out var v)) return v;
            if (double.TryParse(obj.ToString(), out var dv)) return (int)(dv * 100);
            return fallback;
        }

        public static int AsDecimal(object? obj, int fallback = 0)
        {
            if (obj == null) return fallback;
            if (decimal.TryParse(obj.ToString(), out var v)) return (int)(v * 100);
            if (double.TryParse(obj.ToString(), out var dv)) return (int)(dv * 100);
            return fallback;
        }
    }
}

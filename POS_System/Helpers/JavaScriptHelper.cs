namespace POS_System.Helpers
{
    public static class JavaScriptHelper
    {
        public static string JavaScriptStringEncode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return System.Text.Json.JsonSerializer.Serialize(value)
                .Trim('"')
                .Replace("\\", "\\\\")
                .Replace("'", "\\'");
        }
    }
}

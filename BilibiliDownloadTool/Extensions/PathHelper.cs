namespace BilibiliDownloadTool.Extensions
{
    public static class PathHelper
    {
        public static string ToValidPathString(this string input) => input.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
    }
}

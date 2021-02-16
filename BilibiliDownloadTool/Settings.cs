using Windows.Storage;

namespace BilibiliDownloadTool
{
    public static class Settings
    {
        public static string DownloadPath
        {
            get => ApplicationData.Current.LocalSettings.Values["DownloadPath"] as string;
            set => ApplicationData.Current.LocalSettings.Values["DownloadPath"] = value;
        }
        public static string SESSDATA
        {
            get => ApplicationData.Current.LocalSettings.Values["SESSDATA"] as string;
            set => ApplicationData.Current.LocalSettings.Values["SESSDATA"] = value;
        }
        public static long Uid
        {
            get => GetValue<long>("Uid");
            set => SetValue("Uid", value);
        }
        public static bool AutoDlDanmaku
        {
            get => GetValue<bool>("AutoDlDanmaku");
            set => SetValue("AutoDlDanmaku", value);
        }
        public static bool CompleteNotice
        {
            get => GetValue<bool>("CompleteNotice");
            set => SetValue("CompleteNotice", value);
        }
        public static bool NeedShowConsole
        {
            get => GetValue<bool>(nameof(NeedShowConsole));
            set => SetValue(nameof(NeedShowConsole), value);
        }
        private static T GetValue<T>(string name) where T : struct
        {
            var v = ApplicationData.Current.LocalSettings.Values[name];
            if (v == null) return default;
            return (T)v;
        }
        private static void SetValue<T>(string name, T value) where T : struct
            => ApplicationData.Current.LocalSettings.Values[name] = value;
    }
}

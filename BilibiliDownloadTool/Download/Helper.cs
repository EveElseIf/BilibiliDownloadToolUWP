using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace BilibiliDownloadTool.Download
{
    public static class Helper
    {
        public static void ShowVideoCompleteNotice(IBiliDownload download)
        {
            var content = new ToastContentBuilder().AddToastActivationInfo("downloadCompleted", ToastActivationType.Foreground)
                            .AddText("下载完成")
                            .AddText($"{download.Title} - {download.DownloadName}")
                            .AddButton("播放", ToastActivationType.Foreground, $"video\n{download.OutputPath}")
                            .GetToastContent();
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(content.GetXml()));
        }
        public static void ShowMangaCompleteNotice(IBiliDownload download)
        {
            var content = new ToastContentBuilder()
                .AddText("下载完成")
                .AddText($"{download.Title} - {download.DownloadName}")
                .AddButton("查看", ToastActivationType.Foreground, $"manga\n{download.OutputPath}")
                .GetToastContent();
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(content.GetXml()));
        }
    }
}

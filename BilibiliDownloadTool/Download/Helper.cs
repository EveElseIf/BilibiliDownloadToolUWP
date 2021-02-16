using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace BilibiliDownloadTool.Download
{
    public static class Helper
    {
        public static void ShowCompleteNotice(IBiliDownload download)
        {
            var content = new ToastContentBuilder().AddToastActivationInfo("downloadCompleted", ToastActivationType.Foreground)
                            .AddText("下载完成")
                            .AddText($"{download.Title} - {download.DownloadName}")
                            .AddButton("播放", ToastActivationType.Foreground, $"video\n{download.OutputPath}")
                            .GetToastContent();
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(content.GetXml()));

        }
    }
}

using BilibiliDownloadTool.Core.Video;
using BilibiliDownloadTool.Dialogs;
using BilibiliDownloadTool.Download;
using BilibiliDownloadTool.Pages;
using NLog;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Services
{
    public static class VideoDownloadManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static async Task CreateDashDownloadAsync(BiliVideo video, DashVideoInfo videoInfo, DashAudioInfo audioInfo)
        {
            try
            {
                var download = await BiliDashDownload.CreateAsync(video, videoInfo, audioInfo, Settings.SESSDATA);
                DownloadPage.Current.CreateDownload(download);
                download.Completed += async (s, e) =>
                {
                    await LogCompleteAsync(s as BiliDashDownload);
                    _logger.Info($"下载完成 {video.Title}-{video.Name}({video.Bv},P{video.Order})");
                };
                var task = download.StartAsync();
                _logger.Info($"开始下载 {video.Title}-{video.Name}({video.Bv},P{video.Order})");
                await task;
            }
            catch (WebException e)
            {
                _logger.Info(e, e.Message);
                var dialog = new ExceptionDialog(e);
                await dialog.ShowAsync();
                return;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                var dialog = new ExceptionDialog(e);
                await dialog.ShowAsync();
                return;
            }
        }
        private static async Task LogCompleteAsync(IBiliDownload download)
        {
            var item = new Entities.CompleteItem()
            {
                Name = download.DownloadName,
                Title = download.Title,
                Path = download.OutputPath,
                Size = download.FullProgress
            };
            item.Id = (int)await App.SqliteData.AddCompleteItem(item);
            var group = DownloadPage.Current.Groups;
            if (group.Any(g => g.Title == download.Title))
            {
                group.Single(g => g.Title == download.Title).Completes.Add((Complete)item);
            }
            else
            {
                group.Add(new CompleteGroup()
                {
                    Title = download.Title,
                    Completes = new System.Collections.ObjectModel.ObservableCollection<Complete>()
                    {
                        (Complete)item
                    }
                });
            }
        }
        public static async Task CancelDashDownloadAsync(IBiliDownload download)
        {
            try
            {
                await download.CancelAsync();
                DownloadPage.Current.RemoveDownload(download);
                _logger.Info($"下载取消 {download.Title}-{download.DownloadName}");
            }
            catch (Exception ex)
            {
                _logger.Info(ex, ex.Message);
                var dialog = new ExceptionDialog(ex);
                await dialog.ShowAsync();
                return;
            }
        }
    }
}

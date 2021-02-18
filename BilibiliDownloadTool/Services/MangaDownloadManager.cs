using BilibiliDownloadTool.Core.Helpers;
using BilibiliDownloadTool.Core.Manga;
using BilibiliDownloadTool.Dialogs;
using BilibiliDownloadTool.Download;
using BilibiliDownloadTool.Pages;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Services
{
    public static class MangaDownloadManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public static async Task CreateMangaDownloadAsync(BiliManga manga,int mcid,string title)
        {
            try
            {
                var download = await BiliMangaDownload.CreateAsync(manga, mcid, title);
                DownloadPage.Current.CreateDownload(download);
                download.Completed += async (s, e) =>
                {
                    await LogCompleteAsync(s as IBiliDownload);
                    _logger.Info($"下载完成 {title}-{manga.Title}({mcid},{manga.Epid})");
                };
                var task = download.StartAsync();
                _logger.Info($"开始下载 {title}-{manga.Title}({mcid},P{manga.Epid})");
                await task;
            }
            catch (WebException e)
            {
                _logger.Info(e, e.Message);
                //var dialog = new ExceptionDialog(e);
                //await dialog.ShowAsync();
                return;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                //var dialog = new ExceptionDialog(e);
                //await dialog.ShowAsync();
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
                Size = download.FullProgress,
                Type = BiliDownloadType.Manga
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
        public static async Task CancelMangaDownloadAsync(IBiliDownload download)
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
                //var dialog = new ExceptionDialog(ex);
                //await dialog.ShowAsync();
                return;
            }
        }
    }
}

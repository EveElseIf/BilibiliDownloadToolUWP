using BilibiliDownloadTool.Core.Helpers;
using BilibiliDownloadTool.Core.Manga;
using BilibiliDownloadTool.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BilibiliDownloadTool.Download
{
    public class BiliMangaDownload : IBiliDownload
    {
        private BiliManga _manga;
        private readonly List<string> _urls;
        private StorageFolder _outputFolder;
        private bool _canDownload = true;
        private bool _cancelRequested = false;

        public string DownloadName { get; private set; }
        public string Title { get; private set; }
        public BiliDownloadType Type { get; } = BiliDownloadType.Manga;
        public string OutputPath { get => _outputFolder.Path; }

        private long _currentProgress;

        public long CurrentProgress
        {
            get => _currentProgress;
            private set
            {
                _currentProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProgress)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSpeed)));
            }
        }
        public long FullProgress { get => _urls.Count; }
        public string CurrentSpeed { get => $"{CurrentProgress}/{FullProgress}"; }

        private BiliDownloadStatus _status;
        public BiliDownloadStatus Status { get => _status; private set { _status = value;PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); } }

        public IList<IBiliDownloadPart> PartList { get => throw new NotImplementedException(); }

        public StorageFolder CacheFolder { get => throw new NotImplementedException(); }

        public event EventHandler<EventArgs> Completed;
        public event PropertyChangedEventHandler PropertyChanged;

        public BiliMangaDownload(BiliManga manga, List<string> urls)
        {
            _manga = manga;
            _urls = urls;
        }
        public static async Task<IBiliDownload> CreateAsync(BiliManga manga, int mcid, string title)
        {
            var list = await BiliMangaHelper.GetDownloadUrlsAsync(manga, mcid, Settings.SESSDATA);
            var download = new BiliMangaDownload(manga, list)
            {
                Title = title,
                DownloadName = manga.TitleToString(),
                CurrentProgress = 0,
                Status = BiliDownloadStatus.Starting
            };
            var folderName = $"{title} - {manga.TitleToString()}[{mcid}]";
            download._outputFolder = await (await StorageFolder.GetFolderFromPathAsync(Settings.DownloadPath)).CreateFolderAsync(folderName, CreationCollisionOption.ReplaceExisting);
            return download;
        }
        public async Task CancelAsync()
        {
            _cancelRequested = true;
        }

        public void PauseOrResume()
        {
            _canDownload = !_canDownload;
            Status = _canDownload ? BiliDownloadStatus.Running : BiliDownloadStatus.Paused;
        }

        public Task RestartAsync()
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync()
        {
            using var client = new HttpClient();
            if (Settings.SESSDATA is not null) client.DefaultRequestHeaders.Add("cookies", $"SESSDATA={Settings.SESSDATA}");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36 Edg/84.0.522.63");
            client.DefaultRequestHeaders.Add("referrer", "https://www.bilibili.com");
            Status = BiliDownloadStatus.Running;
            for (int i = 0; i < FullProgress; i++)
            {
                while (!_canDownload) await Task.Delay(1000);
                if (_cancelRequested) break;

                var file = await _outputFolder.CreateFileAsync($"{i + 1}.png", CreationCollisionOption.ReplaceExisting);
                using var stream = await file.OpenStreamForWriteAsync();
                using var content = await client.GetStreamAsync(_urls[i]);
                await content.CopyToAsync(stream);
                stream.Dispose();
                content.Dispose();
                CurrentProgress = i + 1;
            }
            client.Dispose();
            if(CurrentProgress == FullProgress)
            {
                Status = BiliDownloadStatus.Completed;
                if (Settings.CompleteNotice)//如果需要通知，就发送下载完成通知
                    Helper.ShowMangaCompleteNotice(this);
                Completed?.Invoke(this, null);
            }
            DownloadPage.Current.RemoveDownload(this);
        }
    }
}

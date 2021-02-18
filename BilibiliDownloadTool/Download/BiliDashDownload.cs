using BilibiliDownloadTool.Core.Video;
using BilibiliDownloadTool.Extensions;
using BilibiliDownloadTool.Pages;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace BilibiliDownloadTool.Download
{
    public class BiliDashDownload : IBiliDownload
    {
        public string DownloadName { get; private set; }
        public string Title { get; private set; }
        public BiliDownloadType Type { get; private set; } = BiliDownloadType.Video;
        #region binding properties
        private long currentProgress;

        public long CurrentProgress
        {
            get { return currentProgress; }
            private set { currentProgress = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProgress))); }
        }
        private long fullProgress;

        public long FullProgress
        {
            get { return fullProgress; }
            private set { fullProgress = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullProgress))); }
        }

        public string CurrentSpeed
        {
            get
            {
                var input = speed;
                if (input >= 1000000) return $"{string.Format("{0:f2}", (double)input / 1000000)}MB/s";
                else return $"{string.Format("{0:f2}", (double)input / 1000)}KB/s";
            }
        }
        private long speed;
        private long Speed
        {
            get => speed;
            set { speed = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSpeed))); }
        }
        private BiliDownloadStatus status;

        public BiliDownloadStatus Status
        {
            get { return status; }
            set { status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); }
        }

        #endregion
        public IList<IBiliDownloadPart> PartList { get; private set; }
        public StorageFolder CacheFolder { get; private set; }
        public string OutputPath { get; private set; }

        public event EventHandler<EventArgs> Completed;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly StorageFolder _globalCachePath = ApplicationData.Current.LocalCacheFolder;
        private bool _started = false;
        private bool _completed = false;
        private bool _paused = false;
        private readonly CancellationTokenSource _tokenSource;
        private readonly BiliVideo _video;
        private readonly DashVideoInfo _videoInfo;
        private readonly DashAudioInfo _audioInfo;

        private BiliDashDownload(BiliVideo video, DashVideoInfo videoInfo, DashAudioInfo audioInfo, CancellationTokenSource tokenSource)
        {
            _video = video;
            _videoInfo = videoInfo;
            _audioInfo = audioInfo;
            _tokenSource = tokenSource;
        }
        public static async Task<BiliDashDownload> CreateAsync(BiliVideo video, DashVideoInfo videoInfo, DashAudioInfo audioInfo, string sessdata = null)
        {
            var downloadName = video.Name;
            var cacheFolderName = Guid.NewGuid().ToString();
            var tokenSource = new CancellationTokenSource();

            var downloader = new BackgroundDownloader();
            downloader.SetRequestHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36 Edg/84.0.522.63");
            downloader.SetRequestHeader("referer", "http://www.bilibili.com");
            if (Directory.Exists(Path.Combine(_globalCachePath.Path, cacheFolderName)))
                await (await _globalCachePath.GetFolderAsync(cacheFolderName)).DeleteAsync(StorageDeleteOption.PermanentDelete);
            var cacheFolder = await _globalCachePath.CreateFolderAsync(cacheFolderName);
            var partList = new List<IBiliDownloadPart>()
            {
                await BiliDashDownloadPart.CreateAsync(videoInfo.Url,Guid.NewGuid().ToString(),cacheFolder,downloader,tokenSource),
                await BiliDashDownloadPart.CreateAsync(audioInfo.Url,Guid.NewGuid().ToString(),cacheFolder,downloader,tokenSource),
            };
            var download = new BiliDashDownload(video, videoInfo, audioInfo, tokenSource)
            {
                DownloadName = downloadName,
                Title = video.Title,
                CacheFolder = cacheFolder,
                PartList = partList
            };
            return download;
        }
        private async Task OnCompletedAsync()
        {
            var title = Title.ToValidPathString();
            var downloadName = DownloadName.ToValidPathString();
            //var videoFile = PartList.Where(p => p.CacheFile.Name.Contains("Video"))?.First()?.CacheFile
            //    ?? throw new Exception();
            //var audioFile = PartList.Where(p => p.CacheFile.Name.Contains("Audio"))?.First()?.CacheFile
            //    ?? throw new Exception();
            var file1 = PartList[0].CacheFile;
            var file2 = PartList[1].CacheFile;
            var saveFolder = await StorageFolder.GetFolderFromPathAsync(Settings.DownloadPath);
            var outputFile = await saveFolder.CreateFileAsync($"{title} - {downloadName}.mp4", CreationCollisionOption.ReplaceExisting);
            await outputFile.DeleteAsync(StorageDeleteOption.PermanentDelete);//创建文件再删除获取读写权限

            await MainPage.VideoHelper.MakeDashVideoAsync(file1, file2, outputFile.Path, CacheFolder.Path);
            
            OutputPath = outputFile.Path;
            Status = BiliDownloadStatus.Completed;
            if (Settings.CompleteNotice)//如果需要通知，就发送下载完成通知
                Helper.ShowVideoCompleteNotice(this);
            Completed?.Invoke(this, null);
            DownloadPage.Current.RemoveDownload(this);
        }

        public async Task StartAsync()
        {
            bool isPartAllCompleted() => PartList.All(p => p.Status == BiliDownloadStatus.Completed);

            Status = BiliDownloadStatus.Running;
            var tasks = new List<Task>();
            long cP;
            long fP;
            long lastBytes = 0;
            foreach (var part in PartList)
            {
                tasks.Add(part.StartAsync());
            }
            while (!isPartAllCompleted() || FullProgress != CurrentProgress)
            {
                if (_tokenSource.IsCancellationRequested) return;
                cP = fP = 0;
                foreach (var part in PartList)
                {
                    if (part.Operation != null)
                    {
                        cP += Convert.ToInt64(part.Operation.Progress.BytesReceived);
                        fP += Convert.ToInt64(part.Operation.Progress.TotalBytesToReceive);
                    }
                }
                if (cP == 0)
                {
                    FullProgress = 100;
                    CurrentProgress = 0;
                    await Task.Delay(1000);
                    continue;
                }
                CurrentProgress = cP;
                FullProgress = fP;
                if (lastBytes != 0) Speed = cP - lastBytes;
                lastBytes = cP;
                await Task.Delay(1000);
            }
            await Task.WhenAll(tasks);
            Status = BiliDownloadStatus.Converting;
            await OnCompletedAsync();
        }

        public Task RestartAsync()
        {
            throw new NotImplementedException();
        }

        public async Task CancelAsync()
        {
            _tokenSource.Cancel();
            await CacheFolder.DeleteAsync();
        }

        public void PauseOrResume()
        {
            if (Status == BiliDownloadStatus.Completed || Status == BiliDownloadStatus.Converting || Status == BiliDownloadStatus.Starting)
                return;
            foreach (var item in PartList)
            {
                item.PauseOrResume();
            }
            if (Status == BiliDownloadStatus.Running)
                Status = BiliDownloadStatus.Paused;
            else
                Status = BiliDownloadStatus.Running;
        }
    }
}

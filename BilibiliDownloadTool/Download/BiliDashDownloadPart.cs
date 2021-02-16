using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace BilibiliDownloadTool.Download
{
    public class BiliDashDownloadPart : IBiliDownloadPart
    {
        private readonly CancellationTokenSource _tokenSource;
        private bool _recreate = false;

        public DownloadOperation Operation { get; private set; }
        public Guid OperationGuid { get; private set; }
        public string Url { get; private set; }
        public StorageFile CacheFile { get; private set; }
        public BiliDownloadStatus Status { get; private set; }

        private BiliDashDownloadPart(StorageFile cacheFile, CancellationTokenSource tokenSource)
        {
            CacheFile = cacheFile;
            _tokenSource = tokenSource;
        }

        public static async Task<BiliDashDownloadPart> CreateAsync(string url, string name, StorageFolder cacheFolder, BackgroundDownloader downloader, CancellationTokenSource tokenSource)
        {
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri)) throw new ArgumentException();
            var cacheFile = await cacheFolder.CreateFileAsync(name);
            var part = new BiliDashDownloadPart(cacheFile, tokenSource)
            {
                Operation = downloader.CreateDownload(uri, cacheFile)
            };
            part.OperationGuid = part.Operation.Guid;
            return part;
        }
        public static async Task<BiliDashDownloadPart> RecreateAsync()
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync()
        {
            if (_recreate)
            {
                if (Operation == null) return;
                Status = BiliDownloadStatus.Running;
                var task = Operation.AttachAsync().AsTask(_tokenSource.Token);
                await task;
            }
            else
            {
                Status = BiliDownloadStatus.Running;
                var task = Operation.StartAsync().AsTask(_tokenSource.Token);
                await task;
            }
            Status = BiliDownloadStatus.Completed;
        }

        public void PauseOrResume()
        {
            if (_recreate)
            {
                Operation?.Resume();
                _recreate = false;
                return;
            }
            if (Operation?.Progress.Status == BackgroundTransferStatus.PausedByApplication)
            {
                Operation.Resume();
                Status = BiliDownloadStatus.Running;
            }
            if (Operation?.Progress.Status == BackgroundTransferStatus.Running)
            {
                Operation?.Pause();
                Status = BiliDownloadStatus.Paused;
            }
        }
    }
}

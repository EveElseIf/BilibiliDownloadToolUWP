using System;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace BilibiliDownloadTool.Download
{
    public interface IBiliDownloadPart
    {
        Task StartAsync();
        void PauseOrResume();
        DownloadOperation Operation { get; }
        Guid OperationGuid { get; }
        string Url { get; }
        StorageFile CacheFile { get; }
        BiliDownloadStatus Status { get; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace BilibiliDownloadTool.Download
{
    public interface IBiliDownload : INotifyPropertyChanged
    {
        Task StartAsync();
        Task RestartAsync();
        Task CancelAsync();
        void PauseOrResume();
        string DownloadName { get; }
        string Title { get; }
        /// <summary>
        /// 下载完成后再访问该属性,否则会返回null
        /// </summary>
        string OutputPath { get; }
        long CurrentProgress { get; }
        long FullProgress { get; }
        long CurrentSpeed { get; }
        BiliDownloadStatus Status { get; }
        IList<IBiliDownloadPart> PartList { get; }
        StorageFolder CacheFolder { get; }
        event EventHandler<EventArgs> Completed;
    }
}

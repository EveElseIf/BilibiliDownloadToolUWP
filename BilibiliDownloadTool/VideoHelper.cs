using BilibiliDownloadTool.Dialogs;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;

namespace BilibiliDownloadTool
{
    public class VideoHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //        private static readonly object _locker = new object();
        //        public static async Task MakeVideoAsync(StorageFile video, StorageFile audio, string outputPath)
        //        {
        //            lock (_locker)//使用锁来防止ffmpeg卡bug
        //            {
        //                Locked = true;
        //                ApplicationData.Current.LocalSettings.Values["lock"] = true;//锁定，这是不同程序之间通信用的锁
        //                string para;
        //                para = $"-i \"{video.Path}\" -i \"{audio.Path}\" -c:v copy -c:a copy -strict experimental \"{outputPath}\"";
        //                ApplicationData.Current.LocalSettings.Values["para"] = para;
        //                if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
        //                {
        //#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        //                    FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("VideoGroup");
        //#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        //                }
        //                while ((bool)ApplicationData.Current.LocalSettings.Values["lock"]) Thread.Sleep(100);
        //                Locked = false;
        //            }
        //        }
        public static async Task<VideoHelper> InitAsync()
        {
            if (App.Connection == null)
            {
                _logger.Info("BilibiliDownloadTool.Launcher启动中");
                if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
                {
                    try
                    {
                        await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                        int i = 0;
                        while (true)
                        {
                            await Task.Delay(100);
                            i++;
                            if (i >= 50) throw new Exception();
                            if (App.Connection != null) break;
                        }
                        _logger.Info("BilibiliDownloadTool.Launcher启动成功");
                        App.AppServiceDisconnected += App_AppServiceDisconnected;
                        return new VideoHelper();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "BilibiliDownloadTool.Launcher启动失败");
                        throw new Exception();
                    }
                }
            }
            throw new InvalidOperationException();
        }

        private static async void App_AppServiceDisconnected(object sender, EventArgs e)
        {
            await MainPage.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                var dialog = new NoticeDialog("警告", "视频合并辅助进程被关闭，这将无法进行视频合并，请重启程序", "关闭程序", "忽略(不推荐)");
                if (await dialog.ShowAsync() == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    App.Current.Exit();
                }
            });            
        }

        public async Task MakeDashVideoAsync(StorageFile video, StorageFile audio, string outputPath,string cacheFolderPath)
        {
            var para = $"-i \"{video.Path}\" -i \"{audio.Path}\" -c:v copy -c:a copy -strict experimental \"{outputPath}\"";
            try
            {
                _logger.Info($"开始合并{video.Name}-{audio.Name}");
                await App.Connection.SendMessageAsync(new Windows.Foundation.Collections.ValueSet()
                    {
                        {"ffmpeg", para+"\n"+cacheFolderPath }
                    });
                _logger.Info($"合并成功，输出到{outputPath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"合并失败");
            }
        }
    }
}

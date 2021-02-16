using BilibiliDownloadTool.Core.Helpers;
using BilibiliDownloadTool.Core.Video;
using BilibiliDownloadTool.Dialogs;
using BilibiliDownloadTool.Services;
using DanmakuToAss.Library;
using Microsoft.UI.Xaml.Controls;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages.ResultPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BiliVideoMasterResultPage : Windows.UI.Xaml.Controls.Page
    {
        private HttpClient _client;
        private BiliVideoMaster _master;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private string Title { get; set; }
        public BiliVideoMasterResultPage()
        {
            this.InitializeComponent();
            DataContext = this;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _master = (BiliVideoMaster)e.Parameter;
            Title = _master.Title;
            videoListView.ItemsSource = _master.Videos;
            try
            {
                CoverImage.Source = new BitmapImage(new Uri(_master.CoverUrl));
            }
            catch (Exception ex)
            {
                _logger.Info(ex, $"获取封面图片失败：{_master.CoverUrl}");
            }
        }

        private async void DownloadMultipleBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = MutipleDownloadDialog.Create(_master.Videos, v =>
            {
                var item = new MutipleDownloadDialogItem
                {
                    Name = v.Name,
                    ToDownload = false,
                    Oringin = v
                };
                return item;
            }, async (item, quality) =>
            {
                var v = (BiliVideo)item.Oringin;
                var dash = await BiliVideoHelper.GetVideoDashStreamAsync(v, Settings.SESSDATA);
                await VideoDownloadManager.CreateDashDownloadAsync(v, dash.TryGetTargetQuality(quality), dash.GetHighestAudioQuality());
            });
            if (await dialog.ShowAsync() == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                ShowTipWithMessage("批量下载任务已创建");
            }
        }

        private async void SingleDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as DropDownButton;
            var video = (BiliVideo)btn.DataContext;
            if (btn.Flyout != null) return;
            btn.Content = new ProgressRing() { IsActive = true, Height = 20, Width = 20 };

            BiliVideoDashInfo dashInfo;
            try
            {
                dashInfo = await BiliVideoHelper.GetVideoDashStreamAsync((BiliVideo)btn.DataContext, Settings.SESSDATA);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                btn.Content = "错误";
                return;
            }

            btn.Content = "下载";
            var flyout = new Windows.UI.Xaml.Controls.MenuFlyout();
            var items = flyout.Items;
            foreach (var info in dashInfo.Videos)
            {
                var item = new Windows.UI.Xaml.Controls.MenuFlyoutItem()
                {
                    Text = $"{info.Quality.ToQualityString()}-{info.Codec}",
                    DataContext = info
                };
                item.Click += async (s, args) =>
                {
                    var task = VideoDownloadManager.CreateDashDownloadAsync
                    (video,
                    (DashVideoInfo)item.DataContext,
                    dashInfo.Audios.OrderByDescending(a => a.Quality).First());
                    ShowTipWithMessage($"{video.Name}-{info.Quality.ToQualityString()}-{info.Codec}");
                    await task;
                };
                items.Add(item);
            }
            btn.Flyout = flyout;
            flyout.ShowAt(btn);
        }
        private void ShowTipWithMessage(string message)
        {
            Tip.Subtitle = message;
            Tip.IsOpen = true;
        }

        private async void DownloadCoverBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null) _client = new();
            try
            {
                using var stream = await _client.GetStreamAsync(_master.CoverUrl);

                var folder = await StorageFolder.GetFolderFromPathAsync(Settings.DownloadPath);
                var file = await folder.CreateFileAsync(_master.Title + "-封面.png", CreationCollisionOption.ReplaceExisting);
                using var fs = await file.OpenStreamForWriteAsync();
                await stream.CopyToAsync(fs);
                stream.Dispose();
                fs.Dispose();

                Tip2.IsOpen = true;
                _logger.Info($"下载了{_master.Bv}的封面");
            }
            catch (WebException ex)
            {
                _logger.Info(ex, ex.Message);
                var dialog = new ExceptionDialog(ex);
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                var dialog = new ExceptionDialog(ex);
                await dialog.ShowAsync();
            }
        }

        private async void DownloadDanmakuXmlBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach(var v in _master.Videos)
            {
                var xml = await BiliDanmakuHelper.GetDanmakuXmlStringAsync(v.Cid);
            }
        }

        private async void DownloadDanmakuAssBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var v in _master.Videos)
            {
                var xml = await BiliDanmakuHelper.GetDanmakuXmlStringAsync(v.Cid);
                var ass = DanmakuParser.LoadXmlFromString(xml).ToAss(1920, 1080);
            }
        }
    }
}

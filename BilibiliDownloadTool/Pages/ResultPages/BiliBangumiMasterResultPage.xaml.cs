using BilibiliDownloadTool.Core.Bangumi;
using BilibiliDownloadTool.Core.Helpers;
using BilibiliDownloadTool.Core.Video;
using BilibiliDownloadTool.Dialogs;
using BilibiliDownloadTool.Services;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages.ResultPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BiliBangumiMasterResultPage : Page
    {
        public string Title { get; private set; }
        private BiliBangumiMaster _master;
        public BiliBangumiMasterResultPage()
        {
            this.InitializeComponent();
            DataContext = this;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is not BiliBangumiMaster) return;
            _master = (BiliBangumiMaster)e.Parameter;
            Title = _master.Title;
            BangumiListView.ItemsSource = _master.EpisodeList;
            try
            {
                CoverImage.Source = new BitmapImage(new Uri(_master.CoverUrl));
            }
            catch
            {

            }
        }

        private async void DownloadMultipleBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = MutipleDownloadDialog.Create(_master.EpisodeList,
                b =>
                {
                    var item = new MutipleDownloadDialogItem()
                    {
                        Name = b.Name,
                        Oringin = b,
                        ToDownload = false
                    };
                    return item;
                },
                async (item, quality) =>
                {
                    var b = (BiliBangumi)item.Oringin;
                    var dash = await BiliVideoHelper.GetVideoDashStreamAsync(b.Video, Settings.SESSDATA);
                    await VideoDownloadManager.CreateDashDownloadAsync(b.Video, dash.TryGetTargetQuality(quality), dash.GetHighestAudioQuality());
                });
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                ShowTipWithMessage("批量下载任务已创建");
            }
        }

        private async void SingleDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Microsoft.UI.Xaml.Controls.DropDownButton;
            var bangumi = (BiliBangumi)btn.DataContext;
            if (btn.Flyout != null) return;
            btn.Content = new ProgressRing() { IsActive = true, Height = 20, Width = 20 };

            BiliVideoDashInfo dashInfo;
            try
            {
                dashInfo = await BiliVideoHelper.GetVideoDashStreamAsync(bangumi.Video);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                btn.Content = "错误";
                return;
            }

            btn.Content = "下载";
            var flyout = new MenuFlyout();
            var items = flyout.Items;
            foreach (var info in dashInfo.Videos)
            {
                var item = new MenuFlyoutItem()
                {
                    Text = $"{info.Quality.ToQualityString()}-{info.Codec}",
                    DataContext = info
                };
                item.Click += async (s, e) =>
                 {
                     await VideoDownloadManager.CreateDashDownloadAsync(bangumi.Video,
                         (DashVideoInfo)item.DataContext,
                         dashInfo.Audios.OrderByDescending(a => a.Quality).First());
                     ShowTipWithMessage($"{bangumi.Name}-{info.Quality.ToQualityString()}-{info.Codec}");
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
    }
}

using BilibiliDownloadTool.Controls;
using BilibiliDownloadTool.Core.Helpers;
using BilibiliDownloadTool.Core.Manga;
using BilibiliDownloadTool.Download;
using BilibiliDownloadTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages.ResultPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BiliMangaMasterResultPage : Page
    {
        private BiliMangaMaster _master;
        private ObservableCollection<BiliMangaDownload> DownloadList { get; } = new ObservableCollection<BiliMangaDownload>();
        
        public BiliMangaMasterResultPage()
        {
            this.InitializeComponent();
            DataContext = this;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _master = (BiliMangaMaster)e.Parameter;
            try
            {
                CoverImg.Source = new BitmapImage(new Uri(_master.VerticalCoverUrl));
            }
            catch
            {

            }
        }

        private void MangaGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            static void showFlyout(GridViewItem item)
            {
                var flyout = item.ContextFlyout as MangaFlyout;
                flyout.ShowAt(item);
            }

            var item = MangaGridView.ContainerFromItem(e.ClickedItem) as GridViewItem;
            if (item.ContextFlyout is not null)
            {
                showFlyout(item);
                return;
            }
            var manga = (BiliManga)e.ClickedItem;
            var flyout = new MangaFlyout(manga, _master.Mcid, _master.Title, this);
            item.ContextFlyout = flyout;
            showFlyout(item);
        }

        public void ShowTipWithMessage(string message)
        {
            Tip.Subtitle = message;
            Tip.IsOpen = true;
        }
    }
}

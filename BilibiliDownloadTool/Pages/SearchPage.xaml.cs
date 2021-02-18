using BilibiliDownloadTool.Core;
using BilibiliDownloadTool.Dialogs;
using BilibiliDownloadTool.Pages.ResultPages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        private readonly ObservableCollection<TabViewItem> TabViewItems = new ObservableCollection<TabViewItem>();
        public static SearchPage Current { get; private set; }
        public SearchPage()
        {
            this.InitializeComponent();
            Current = this;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            TabView.TabItemsSource = TabViewItems;
            var frame = new Frame();
            frame.Navigate(typeof(SearchHomePage));
            TabViewItems.Add(new TabViewItem()
            {
                Header = "搜索",
                IsClosable = false,
                IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource()
                {
                    Symbol = Symbol.Find
                },
                Content = frame
            });
            TabView.SelectedIndex = 0;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //检查有没有设置下载目录
            if (string.IsNullOrWhiteSpace(Settings.DownloadPath))
            {
                var dialog = new NoticeDialog("请设置下载目录", "使用前必须设置下载目录", "转到“设置”");
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                    NavigationHelper.NavigateToPage(ContentPage.Settings);
                return;
            }
        }
        ~SearchPage()
        {
            Current = null;
        }
        public void HandleMaster<TPage, TMaster>(TMaster master) where TPage : Page where TMaster : IBiliMaster
        {
            var frame = new Frame();
            frame.Navigate(typeof(TPage), master);
            var item = new TabViewItem()
            {
                Header = master.Title,
                IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource()
                {
                    Symbol = typeof(TPage) == typeof(BiliMangaMasterResultPage) ? Symbol.Bookmarks : Symbol.Video
                },
                Content = frame
            };
            TabViewItems.Add(item);
            TabView.SelectedItem = item;
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            TabViewItems.Remove(args.Tab);
        }
    }
}

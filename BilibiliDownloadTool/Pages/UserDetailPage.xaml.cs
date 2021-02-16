using BilibiliDownloadTool.Core.Bangumi;
using BilibiliDownloadTool.Core.Exceptions;
using BilibiliDownloadTool.Core.Fav;
using BilibiliDownloadTool.Core.Helpers;
using BilibiliDownloadTool.Core.Video;
using BilibiliDownloadTool.Dialogs;
using BilibiliDownloadTool.Pages.ResultPages;
using BilibiliDownloadTool.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserDetailPage : Page
    {
        private BiliUser _user;
        private bool _favListViewLoaded = false;
        private bool _bangumiListViewLoaded = false;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public UserDetailPage()
        {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await InitUserAsync();
        }
        private async Task InitUserAsync()
        {
            InitializingProgressRing.IsActive = true;
            _user = await BiliUserManager.GetBiliUserAsync(Settings.SESSDATA);
            try
            {
                AvatarImage.Source = new BitmapImage(new Uri(_user.AvatarUrl));
            }
            catch (Exception ex)
            {
                _logger.Info(ex, ex.Message);
            }
            UserNameTextBlock.Text = _user.Name;
            InitializingProgressRing.IsActive = false;
        }
        private void StartLoadingAnimation()
        {
            this.ProgressRingStackPanel.Visibility = Visibility.Visible;
            this.CommonProgressRing.IsActive = true;
        }
        private void StopLoadingAnimation()
        {
            this.ProgressRingStackPanel.Visibility = Visibility.Collapsed;
            this.CommonProgressRing.IsActive = false;
        }
        #region 下面是收藏夹用的
        private async void FavListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_favListViewLoaded) return;
            _favListViewLoaded = true;
            StartLoadingAnimation();
            var favInfoList = await BiliFavHelper.GetUserFavListInfoAsync(Settings.Uid, Settings.SESSDATA);
            var favList = new List<FavViewModel>();
            foreach (var info in favInfoList)
            {
                var fav = await BiliFavHelper.GetBiliFavAsync(info.Id, 1, Settings.SESSDATA);
                if (fav.Id == 0)
                {
                    favList.Add(new FavViewModel()
                    {
                        Id = info.Id,
                        Title = info.Title,
                        VideoCount = 0,
                        VideoList = new ObservableCollection<FavVideoViewModel>()
                    });
                    continue;
                }
                if (fav.VideoMasterCount > 0)
                {
                    favList.Add(new FavViewModel()
                    {
                        Id = fav.Id,
                        Title = fav.Title,
                        VideoCount = fav.VideoMasterCount,
                        VideoList = new ObservableCollection<FavVideoViewModel>()
                    });
                }
            }
            var collection = new ObservableCollection<FavViewModel>(favList);
            FavListView.ItemsSource = collection;
            StopLoadingAnimation();
        }

        private async void FavListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartLoadingAnimation();
            await FavListView_LoadItem(FavListView.SelectedItem as FavViewModel);
            FavVideoGridView.ItemsSource = (FavListView.SelectedItem as FavViewModel).VideoList;
            StopLoadingAnimation();
        }
        private async Task FavListView_LoadItem(FavViewModel vm)
        {
            if (vm.VideoList.Count > 0) return;
            var fav = await BiliFavHelper.GetBiliFavAsync(vm.Id, 1, Settings.SESSDATA);
            if (fav.Id == 0)
            {
                var dialog = new NoticeDialog("提示", "已经没有更多了", "确定");
                await dialog.ShowAsync();
                return;
            }
            var list = new List<FavVideoViewModel>();
            foreach (var video in fav.VideoMasters)
            {
                list.Add(await FavVideoViewModel.CreateAsync(video.Title, video.Bv, video.CoverUrl));
            }
            list.ForEach(v => vm.VideoList.Add(v));
            vm.VideoList.Add(new FavVideoViewModel()
            {
                Bv = "加载更多",
                Title = "加载更多",
                CoverImg = new BitmapImage(new Uri("ms-appx:///Assets/LoadMore.png"))
            });
        }

        private async void FavVideoGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as FavVideoViewModel).Bv == "加载更多")
            {
                var vm = FavListView.SelectedItem as FavViewModel;
                await vm.GetMoreVideoAsync();
            }
            else if (!Regex.IsMatch((e.ClickedItem as FavVideoViewModel).Bv, "[B|b][V|v][0-9]*")) return;
            else
            {
                try
                {
                    var info = e.ClickedItem as FavVideoViewModel;
                    var master = await BiliVideoHelper.GetVideoMasterAsync(info.Bv, Settings.SESSDATA);
                    NavigationHelper.NavigateToPage(ContentPage.Search);
                    SearchPage.Current.HandleMaster<BiliVideoMasterResultPage, BiliVideoMaster>(master);
                }
                catch (ParsingVideoException)
                {
                    var dialog = new NoticeDialog("提示", "该视频已失效");
                    await dialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    _logger.Info(ex, ex.Message);
                }
            }
        }
        #endregion
        #region 下面是追番用的
        private async void BangumiListGridView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_bangumiListViewLoaded) return;
            _bangumiListViewLoaded = true;
            StartLoadingAnimation();
            var list = new List<BangumiViewModel>();
            var infoList = await BiliFavHelper.GetFavBangumiMasterListAsync(1, Settings.Uid, Settings.SESSDATA);
            if (infoList.Count < 1)
            {
                list.Add(new BangumiViewModel()
                {
                    Title = "追番为空",
                    SeasonId = 0
                });
            }
            else
            {
                foreach (var bangumi in infoList)
                {
                    var model = new BangumiViewModel()
                    {
                        Title = bangumi.Title,
                        SeasonId = bangumi.SeasonId
                    };
                    try
                    {
                        model.CoverImg = new BitmapImage(new Uri(bangumi.CoverUrl));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, ex.Message);
                    }
                    list.Add(model);
                }
            }
            list.Add(new BangumiViewModel()
            {
                Title = "加载更多",
                SeasonId = 0,
                //coverimage = ???
            });
            var collection = new ObservableCollection<BangumiViewModel>(list);
            BangumiListGridView.ItemsSource = collection;
            StopLoadingAnimation();
        }

        private async void BangumiListGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var info = e.ClickedItem as BangumiViewModel;
            if (info.SeasonId == 0)
            {
                var list = BangumiListGridView.ItemsSource as ObservableCollection<BangumiViewModel>;
                var count = list.Count - 1;

                async Task NoticeNoMore()
                {
                    var dialog = new NoticeDialog("提示", "已经没有更多了");
                    await dialog.ShowAsync();
                }

                if ((count < 15) || (count % 15 != 0))
                {
                    await NoticeNoMore();
                    return;
                }
                var newList = await BiliFavHelper.GetFavBangumiMasterListAsync((count / 15) + 1, Settings.Uid, Settings.SESSDATA);
                if (newList == null)
                {
                    await NoticeNoMore();
                    return;
                }
                var toAddList = new List<BangumiViewModel>();
                list.Remove(list.Last());
                foreach (var bangumi in newList)
                {
                    var model = new BangumiViewModel()
                    {
                        Title = bangumi.Title,
                        SeasonId = bangumi.SeasonId
                    };
                    try
                    {
                        model.CoverImg = new BitmapImage(new Uri(bangumi.CoverUrl));
                    }
                    catch (Exception ex)
                    {
                        _logger.Info(ex, ex.Message);
                    }
                    toAddList.Add(model);
                }
                toAddList.ForEach(b => list.Add(b));
                list.Add(new BangumiViewModel()
                {
                    Title = "加载更多",
                    SeasonId = 0,
                    //CoverImg = ???
                });
            }
            else
            {
                var master = await BiliBangumiHelper.GetBangumiMasterAsync(info.SeasonId, Settings.SESSDATA);
                NavigationHelper.NavigateToPage(ContentPage.Search);
                SearchPage.Current.HandleMaster<BiliBangumiMasterResultPage, BiliBangumiMaster>(master);
            }
        }
        #endregion

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.SESSDATA = string.Empty;
            Settings.Uid = 0;
            UserPage.Current.Logout();
        }
    }

    public class FavViewModel
    {
        public static async Task<FavViewModel> CreateAsync(BiliFav fav)
        {
            var model = new FavViewModel()
            {
                Title = fav.Title,
                Id = fav.Id
            };

            var list = new ObservableCollection<FavVideoViewModel>();

            foreach (var video in fav.VideoMasters)
            {
                list.Add(await FavVideoViewModel.CreateAsync(video.Title, video.Bv, video.CoverUrl));
            }

            list.Add(new FavVideoViewModel()
            {
                Bv = "加载更多",
                Title = "加载更多",
                CoverImg = new BitmapImage(new Uri("ms-appx:///Assets/LoadMore.png"))
            });

            model.VideoList = list;

            return model;
        }
        public async Task GetMoreVideoAsync()
        {
            var count = this.VideoList.Count - 1;
            if ((count < 20) || (count % 20 != 0))
            {
                var dialog = new NoticeDialog("提示", "已经没有更多了", "确定");
                await dialog.ShowAsync();
                return;
            }

            var fav = await BiliFavHelper.GetBiliFavAsync(this.Id, (count / 20) + 1, Settings.SESSDATA);

            if (fav.Id == 0)
            {
                var dialog = new NoticeDialog("提示", "已经没有更多了", "确定");
                await dialog.ShowAsync();
                return;
            }

            var list = new List<FavVideoViewModel>();
            foreach (var video in fav.VideoMasters)
            {
                list.Add(await FavVideoViewModel.CreateAsync(video.Title, video.Bv, video.CoverUrl));
            }

            this.VideoList.Remove(this.VideoList.TakeLast(1).Single());
            list.ForEach(v => this.VideoList.Add(v));

            this.VideoList.Add(new FavVideoViewModel()
            {
                Bv = "加载更多",
                Title = "加载更多",
                CoverImg = new BitmapImage(new Uri("ms-appx:///Assets/LoadMore.png"))
            });
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public int VideoCount { get; set; }
        public ObservableCollection<FavVideoViewModel> VideoList { get; set; }
    }

    public class FavVideoViewModel
    {
        public static async Task<FavVideoViewModel> CreateAsync(string title, string bv, string coverUrl)
        {
            var model = new FavVideoViewModel
            {
                Title = title,
                Bv = bv
            };

            try
            {
                model.CoverImg = new BitmapImage(new Uri(coverUrl));
            }
            catch (System.Exception ex)//封面下载不了的异常
            {
                model.CoverImg = new BitmapImage(new Uri("ms-appx:///Assets/LockScreenLogo.scale-200.png"));
            }
            return model;
        }
        public string Bv { get; set; }
        public string Title { get; set; }
        public ImageSource CoverImg { get; set; }
    }
    public class BangumiViewModel
    {
        public int SeasonId { get; set; }
        public string Title { get; set; }
        public ImageSource CoverImg { get; set; }
    }

}

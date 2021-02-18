using BilibiliDownloadTool.Dialogs;
using BilibiliDownloadTool.Download;
using BilibiliDownloadTool.Extensions;
using BilibiliDownloadTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownloadPage : Page
    {
        public static DownloadPage Current { get; private set; }
        private readonly ObservableCollection<IBiliDownload> _activeDownloads;
        public ObservableCollection<CompleteGroup> Groups { get; set; }
        private bool _inited = false;
        public DownloadPage()
        {
            if (Current != this) Current = this;
            NavigationCacheMode = NavigationCacheMode.Required;
            this.InitializeComponent();
            DownloadListView.ItemsSource = _activeDownloads = new ObservableCollection<IBiliDownload>();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!_inited)
            {
                var data = await App.SqliteData.GetCompleteItems();
                var group = from c in data
                            group c by c.Title into g
                            select g;
                var groups = group.Select(g => new CompleteGroup()
                {
                    Title = g.Key,
                    Completes = g.Select(i => (Complete)i).ToObservableCollection()
                });
                Groups = groups.ToObservableCollection();
                _inited = true;
            }
            base.OnNavigatedTo(e);
        }
        public void CreateDownload(IBiliDownload download)
        {
            _activeDownloads.Add(download);
        }
        public void RemoveDownload(IBiliDownload download)
        {
            if (_activeDownloads.Contains(download))
                _activeDownloads.Remove(download);
        }

        private async void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var download = (sender as Button).DataContext as IBiliDownload;
                await VideoDownloadManager.CancelDashDownloadAsync(download);
            }
            catch (Exception ex)
            {
                var dialog = new ExceptionDialog(ex);
                await dialog.ShowAsync();
            }
        }

        private async void PauseOrResumeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var download = (sender as Button).DataContext as IBiliDownload;
                download.PauseOrResume();
            }
            catch (Exception ex)
            {
                var dialog = new ExceptionDialog(ex);
                await dialog.ShowAsync();
            }
        }

        private async void CompleteItemPlayBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as Complete;
            if(item.Type == BiliDownloadType.Video)
                _ = Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(item.Path));
            if (item.Type == BiliDownloadType.Manga)
                _ = Launcher.LaunchFolderPathAsync(item.Path);
        }

        private void CompleteItemFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            _ = Launcher.LaunchFolderPathAsync(Directory.GetParent(((sender as HyperlinkButton).DataContext as Complete).Path).FullName);
        }

        private void CompleteItemRemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as Complete;
            var group = Groups.Where(g => g.Completes.Contains(item)).Single();
            group.Completes.Remove(item);
            if (group.Completes.Count == 0) Groups.Remove(group);
            _ = App.SqliteData.DeleteCompleteItem(item.Id);
        }
    }
    public class Complete
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public int Id { get; set; }
        public BiliDownloadType Type { get; set; }
    }
    public class CompleteGroup
    {
        public string Title { get; set; }
        public ObservableCollection<Complete> Completes { get; set; }
    }
    public class BiliDownloadStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (BiliDownloadStatus)value;
            return status switch
            {
                BiliDownloadStatus.Starting => "创建中",
                BiliDownloadStatus.Running => "下载中",
                BiliDownloadStatus.Paused => "已暂停",
                BiliDownloadStatus.Converting => "转换中",
                BiliDownloadStatus.Completed => "已完成",
                _ => null,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class DownloadSpeedToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var input = (long)value;
            if (input >= 1000000) return $"{string.Format("{0:f2}", (double)input / 1000000)}MB/s";
            else return $"{string.Format("{0:f2}", (double)input / 1000)}KB/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class BiliDownloadStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (BiliDownloadStatus)value;
            return status switch
            {
                BiliDownloadStatus.Starting => char.ConvertFromUtf32(0xE103),
                BiliDownloadStatus.Running => char.ConvertFromUtf32(0xE103),
                BiliDownloadStatus.Paused => char.ConvertFromUtf32(0xE102),
                BiliDownloadStatus.Converting => char.ConvertFromUtf32(0xE103),
                BiliDownloadStatus.Completed => char.ConvertFromUtf32(0xE103),
                _ => throw new NotImplementedException(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class SizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var input = (long)value;
            if (input >= 1000000) return $"{string.Format("{0:f2}", (double)input / 1000000)}MB";
            else return $"{string.Format("{0:f2}", (double)input / 1000)}KB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class CountToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var input = (int)value;
            return $"共包含{input}个视频";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

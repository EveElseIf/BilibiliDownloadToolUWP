using BilibiliDownloadTool.Core.Video;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BilibiliDownloadTool.Dialogs
{
    public sealed partial class MutipleDownloadDialog : ContentDialog
    {
        private readonly MutipleDownloadDialogViewModel _vm;
        private readonly Func<MutipleDownloadDialogItem, BiliVideoQuality, Task> _downloader;
        public List<ComboBoxData> ComboBoxDatas = new List<ComboBoxData>()
        {
            new ComboBoxData("360P 流畅",BiliVideoQuality.Q360P),
            new ComboBoxData("480P 清晰",BiliVideoQuality.Q480P ),
            new ComboBoxData("720P 高清(需要登录)",BiliVideoQuality.Q720P ),
            new ComboBoxData("720P60 高清(大会员)" ,BiliVideoQuality.Q720P60),
            new ComboBoxData("1080P 高清 (需要登录)" ,BiliVideoQuality.Q1080P),
            new ComboBoxData("1080P+ 高清(大会员)" ,BiliVideoQuality.Q1080PPlus),
            new ComboBoxData("1080P60 高清(大会员)" ,BiliVideoQuality.Q1080P60),
            new ComboBoxData("4K 超清(大会员)" ,BiliVideoQuality.Q4K)
        };

        private MutipleDownloadDialog(MutipleDownloadDialogViewModel vm, Func<MutipleDownloadDialogItem, BiliVideoQuality, Task> downloader)
        {
            _vm = vm;
            _downloader = downloader;
            this.InitializeComponent();
        }
        public static MutipleDownloadDialog Create<T>(IEnumerable<T> Items, Func<T, MutipleDownloadDialogItem> converter, Func<MutipleDownloadDialogItem, BiliVideoQuality, Task> downloader)
        {
            var vm = new MutipleDownloadDialogViewModel
            {
                Collection = new ObservableCollection<MutipleDownloadDialogItem>()
            };
            foreach (var item in Items)
            {
                vm.Collection.Add(converter.Invoke(item));
            }
            var dialog = new MutipleDownloadDialog(vm, downloader);
            return dialog;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var tasks = new List<Task>();
            foreach (var item in _vm.Collection)
            {
                if (item.ToDownload)
                    tasks.Add(_downloader.Invoke(item, (BiliVideoQuality)QualityComboBox.SelectedValue));
            }
            await Task.WhenAll(tasks);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _vm.Collection)
            {
                item.ToDownload = (bool)SelectAllCheckBox.IsChecked;
            }
        }
    }
    public class MutipleDownloadDialogViewModel
    {
        public string Title { get; set; } = "批量下载";
        public ObservableCollection<MutipleDownloadDialogItem> Collection { get; set; }
    }
    public class MutipleDownloadDialogItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private bool _toDownload;

        public bool ToDownload
        {
            get { return _toDownload; }
            set { _toDownload = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToDownload))); }
        }

        public object Oringin { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class ComboBoxData
    {
        public ComboBoxData(string text, BiliVideoQuality value)
        {
            Text = text;
            Value = value;
        }
        public string Text { get; set; }
        public BiliVideoQuality Value { get; set; }
    }
}

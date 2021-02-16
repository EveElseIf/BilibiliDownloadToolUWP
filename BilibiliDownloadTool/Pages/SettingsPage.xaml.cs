using BilibiliDownloadTool.Dialogs;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!string.IsNullOrWhiteSpace(Settings.DownloadPath))
                DownloadPathTextBox.Text = Settings.DownloadPath;
            AutoDlDanmakuSwitch.IsOn = Settings.AutoDlDanmaku;
            NoticeSwitch.IsOn = Settings.CompleteNotice;
            ShowConsoleSwitch.IsOn = Settings.NeedShowConsole;
            RootDirTextBlock.Text = Directory.GetParent(ApplicationData.Current.LocalFolder.Path).FullName;
            InitLogsComboBox();
        }
        private async void InitLogsComboBox()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var files = (await folder.GetFilesAsync()).OrderBy(f => f.DateCreated).ToList();
            files.ForEach(f =>
            {
                if (f.Name != "data.db")
                    LogsComboBox.Items.Add(f);
            });
        }

        private async void ChangeDownloadPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };
            picker.FileTypeFilter.Add("*");
            var folder = await picker.PickSingleFolderAsync();
            if (folder == null) return;
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("DownloadPath", folder);
            Settings.DownloadPath = folder.Path;
            DownloadPathTextBox.Text = folder.Path;
        }

        private void AutoDlDanmakuSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.AutoDlDanmaku = AutoDlDanmakuSwitch.IsOn;
        }

        private async void OpenLogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LogsComboBox.SelectedItem == null) return;
            var file = LogsComboBox.SelectedItem as StorageFile;
            await Launcher.LaunchFileAsync(file);
        }

        private void NoticeSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.CompleteNotice = NoticeSwitch.IsOn;
        }

        private void RootDirBtn_Click(object sender, RoutedEventArgs e)
        {
            _ = Launcher.LaunchFolderPathAsync(RootDirTextBlock.Text);
        }

        private void ShowConsoleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.NeedShowConsole = ShowConsoleSwitch.IsOn;
        }
    }
}

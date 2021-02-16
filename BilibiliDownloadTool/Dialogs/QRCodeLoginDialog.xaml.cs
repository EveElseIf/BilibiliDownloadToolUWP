using BilibiliDownloadTool.Services;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BilibiliDownloadTool.Dialogs
{
    public sealed partial class QRCodeLoginDialog : ContentDialog
    {
        private string _sessdata;
        private long _uid;
        public QRCodeLoginDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Settings.SESSDATA = _sessdata;
            Settings.Uid = _uid;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void GetQRcodeBtn_Click(object sender, RoutedEventArgs e)
        {
            GetQRcodeBtn.IsEnabled = false;
            GetQRcodeBtn.Content = new ProgressRing()
            {
                IsActive = true,
                Height = 100,
                Width = 100
            };
            var tokenSource = new CancellationTokenSource();
            var result = await BiliUserManager.LoginWithQRCodeAsync(QRcodeImage, tokenSource.Token);
            if (result == null) return;//登录出错
            _sessdata = result[0];
            _uid = long.Parse(result[1]);
            IsPrimaryButtonEnabled = true;
        }
    }
}

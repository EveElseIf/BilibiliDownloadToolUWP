using BilibiliDownloadTool.Services;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BilibiliDownloadTool.Dialogs
{
    public sealed partial class SESSDATALoginDialog : ContentDialog
    {
        private string _sessdata;
        public SESSDATALoginDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Settings.SESSDATA = _sessdata;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var input = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(input)) return;
            Btn.Content = new ProgressRing() { IsActive = true };
            Btn.IsEnabled = false;
            if (await BiliUserManager.CheckSESSDATAStatusAsync(input))
            {
                IsPrimaryButtonEnabled = true;
                Btn.Content = "成功";
                _sessdata = input;
            }
            else
            {
                Btn.Content = "失败";
                Btn.IsEnabled = true;
                await Task.Delay(1000);
                Btn.Content = "检验";
            }
        }
    }
}

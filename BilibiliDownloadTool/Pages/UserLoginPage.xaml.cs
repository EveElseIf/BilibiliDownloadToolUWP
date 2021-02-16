using BilibiliDownloadTool.Dialogs;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserLoginPage : Page
    {
        public UserLoginPage()
        {
            this.InitializeComponent();
        }

        private async void SESSDATABtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SESSDATALoginDialog();
            if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
                UserPage.Current.LoginOk();
        }

        private async void QRCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new QRCodeLoginDialog();
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                UserPage.Current.LoginOk();
        }

        private async void PwdBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PwdLoginDialog();
            if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
                UserPage.Current.LoginOk();
        }
    }
}

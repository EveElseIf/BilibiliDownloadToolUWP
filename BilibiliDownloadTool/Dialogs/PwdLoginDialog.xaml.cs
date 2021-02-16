using BilibiliDownloadTool.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BilibiliDownloadTool.Dialogs
{
    public sealed partial class PwdLoginDialog : ContentDialog
    {
        private bool _canClose = false;
        public PwdLoginDialog()
        {
            this.InitializeComponent();
            this.Closing += (s, e) =>
            {
                if (!_canClose) e.Cancel = true;
            };
        }
        

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(PwdBox.Password)) return;
            if (string.IsNullOrWhiteSpace(UserTextBox.Text)) return;
            var user = UserTextBox.Text;
            var pwd = PwdBox.Password;
            BiliAccount.Account account = null;
            await Task.Run(() =>
            {
                account = BiliAccount.Linq.ByPassword.LoginByPassword(user, pwd);
            });
            if (account == null) return;
            switch (account.LoginStatus)
            {
                case BiliAccount.Account.LoginStatusEnum.WrongPassword:
                    StatusTextBlock.Text = "密码错误";
                    break;
                case BiliAccount.Account.LoginStatusEnum.ByPassword:
                    Settings.SESSDATA = account.Cookies["SESSDATA"].Value;
                    Settings.Uid = long.Parse(account.Uid);
                    this._canClose = true;
                    this.Hide();
                    break;
                case BiliAccount.Account.LoginStatusEnum.NeedSafeVerify:
                case BiliAccount.Account.LoginStatusEnum.NeedTelVerify:
                case BiliAccount.Account.LoginStatusEnum.NeedCaptcha:
                case BiliAccount.Account.LoginStatusEnum.None:
                case BiliAccount.Account.LoginStatusEnum.ByQrCode:
                case BiliAccount.Account.LoginStatusEnum.BySMS:
                default:
                    StatusTextBlock.Text = "未知错误，建议更换登录方式";
                    break;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this._canClose = true;
        }
    }
}

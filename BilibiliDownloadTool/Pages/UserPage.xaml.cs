using BilibiliDownloadTool.Services;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserPage : Page
    {
        public static UserPage Current { get; private set; }
        private bool _initialed = false;
        public UserPage()
        {
            NavigationCacheMode = NavigationCacheMode.Enabled;
            if (Current != this) Current = this;
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (_initialed) return;
            _initialed = true;
            if (await BiliUserManager.CheckSESSDATAStatusAsync(Settings.SESSDATA))
                ContentFrame.Navigate(typeof(UserDetailPage));
            else
                ContentFrame.Navigate(typeof(UserLoginPage));
        }
        public void LoginOk()
        {
            ContentFrame.Navigate(typeof(UserDetailPage));
        }
        public void Logout() => ContentFrame.Navigate(typeof(UserLoginPage));
    }
}

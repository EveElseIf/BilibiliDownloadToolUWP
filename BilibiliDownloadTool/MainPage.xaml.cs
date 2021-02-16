using BilibiliDownloadTool.Pages;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace BilibiliDownloadTool
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current { get; private set; }
        public static VideoHelper VideoHelper { get; private set; }
        public MainPage()
        {
            if (Current != this) Current = this;
            this.InitializeComponent();
            NavView.SelectedItem = DownloadItem;

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, e) =>
              UpDateAppTitle(s);
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            VideoHelper = await VideoHelper.InitAsync();
        }

        private void UpDateAppTitle(CoreApplicationViewTitleBar s)
        {
            var margin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(margin.Left, margin.Top, s.SystemOverlayRightInset, margin.Bottom);
        }

        private void NavView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                if (ContentFrame.SourcePageType == typeof(SettingsPage)) return;
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var name = args.InvokedItemContainer.Name.Replace("Item", "Page");
                if (ContentFrame.SourcePageType.Name == name) return;
                switch (args.InvokedItemContainer.Name)
                {
                    case "DownloadItem":
                        ContentFrame.Navigate(typeof(DownloadPage));
                        break;
                    case "SearchItem":
                        ContentFrame.Navigate(typeof(SearchPage));
                        break;
                    case "UserItem":
                        ContentFrame.Navigate(typeof(UserPage));
                        break;
                    default:
                        break;
                }
            }
        }

        public string GetAppTitleFromSystem()
        {
            var a = Windows.ApplicationModel.Package.Current.DisplayName;
            return a;
        }
    }
}

using BilibiliDownloadTool.Pages;

namespace BilibiliDownloadTool
{
    public class NavigationHelper
    {
        public static void NavigateToPage(ContentPage page)
        {
            var navView = MainPage.Current.NavView;
            var contentFrame = MainPage.Current.ContentFrame;
            switch (page)
            {
                case ContentPage.Download:
                    contentFrame.Navigate(typeof(DownloadPage));
                    navView.SelectedItem = navView.MenuItems[0];
                    break;
                case ContentPage.Search:
                    contentFrame.Navigate(typeof(SearchPage));
                    navView.SelectedItem = navView.MenuItems[1];
                    break;
                case ContentPage.User:
                    contentFrame.Navigate(typeof(UserPage));
                    navView.SelectedItem = navView.MenuItems[2];
                    break;
                case ContentPage.Settings:
                    contentFrame.Navigate(typeof(SettingsPage));
                    navView.SelectedItem = navView.SettingsItem;
                    break;
                default:
                    break;
            }
        }
    }
    public enum ContentPage
    {
        Download,
        Search,
        User,
        Settings
    }
}

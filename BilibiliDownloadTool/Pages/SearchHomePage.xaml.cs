using BilibiliDownloadTool.Controls;
using BilibiliDownloadTool.Core.Bangumi;
using BilibiliDownloadTool.Core.Exceptions;
using BilibiliDownloadTool.Core.Helpers;
using BilibiliDownloadTool.Core.Video;
using BilibiliDownloadTool.Pages.ResultPages;
using NLog;
using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BilibiliDownloadTool.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchHomePage : Page
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public SearchHomePage()
        {
            this.InitializeComponent();
            this.SearchTextBox.KeyDown += (s, e) =>
            {
                if (e.Key != Windows.System.VirtualKey.Enter) return;
                SearchBtn_Click(null, null);
            };
        }
        private void SearchProgress()
        {
            ProgressRing.IsActive = true;
            SearchBtn.IsEnabled = false;
            SearchTextBox.IsEnabled = false;
        }
        private void Reset()
        {
            ProgressRing.IsActive = false;
            SearchBtn.IsEnabled = true;
            SearchTextBox.IsEnabled = true;
        }

        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            var searchContent = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchContent)) return;

            var result = AnalyzeUrl(searchContent);
            if (string.IsNullOrWhiteSpace(result.Item2))//输入了无效的url
            {
                ShowNoticeFlyout("输入了无效的url");
                return;
            }
            SearchProgress();
            try
            {
                switch (result.Item1)
                {
                    case UrlType.Bv:
                        var video1 = await BiliVideoHelper.GetVideoMasterAsync(result.Item2, Settings.SESSDATA);
                        SearchPage.Current.HandleMaster<BiliVideoMasterResultPage, BiliVideoMaster>(video1);
                        Reset();
                        break;
                    case UrlType.Av:
                        var video2 = await BiliVideoHelper.GetVideoMasterAsync(long.Parse(result.Item2), Settings.SESSDATA);
                        SearchPage.Current.HandleMaster<BiliVideoMasterResultPage, BiliVideoMaster>(video2);
                        Reset();
                        break;
                    case UrlType.Ep:
                        var bangumi1 = await BiliBangumiHelper.GetBangumiMasterAsync(long.Parse(result.Item2), Settings.SESSDATA, null);
                        SearchPage.Current.HandleMaster<BiliBangumiMasterResultPage, BiliBangumiMaster>(bangumi1);
                        Reset();
                        break;
                    case UrlType.Ss:
                        var bangumi2 = await BiliBangumiHelper.GetBangumiMasterAsync(long.Parse(result.Item2), Settings.SESSDATA);
                        SearchPage.Current.HandleMaster<BiliBangumiMasterResultPage, BiliBangumiMaster>(bangumi2);
                        Reset();
                        break;
                    case UrlType.Mc:
                        break;
                    case UrlType.Error:
                        Reset();
                        break;
                    default:
                        Reset();
                        break;
                }
            }
            catch (Exception ex)
            {
                Reset();
                if (ex is VideoNotFoundException or ArgumentException
                    or UnauthorizedAccessException or ParsingVideoException)
                    ShowNoticeFlyout(ex.Message);
                else
                {
                    ShowNoticeFlyout("发生错误，请查看日志文件");
                    _logger.Error(ex);
                }
                return;
            }
        }
        private void ShowNoticeFlyout(string info)
        {
            var flyout = new NoticeFlyout("提示", info);
            flyout.ShowAt(this.SearchBtn);
        }
        private const string bvRegex = "[B|b][V|v][a-z|A-Z|0-9]*";
        private const string avRegex = "[A|a][V|v][0-9]*";
        private const string ssRegex = "[S|s][S|s][0-9]*";
        private const string epRegex = "[E|e][P|p][0-9]*";
        public (UrlType, string) AnalyzeUrl(string input)
        {
            if (Regex.IsMatch(input, bvRegex)) return (UrlType.Bv, Regex.Match(input, bvRegex).Value);
            if (Regex.IsMatch(input, avRegex)) return (UrlType.Av, Regex.Match(input, avRegex).Value.Substring(2));
            if (Regex.IsMatch(input, ssRegex)) return (UrlType.Ss, Regex.Match(input, ssRegex).Value.Substring(2));
            if (Regex.IsMatch(input, epRegex)) return (UrlType.Ep, Regex.Match(input, epRegex).Value.Substring(2));
            return (UrlType.Error, null);
        }

    }
    public enum UrlType
    {
        Bv,
        Av,
        Ep,
        Ss,
        Mc,
        Error
    }
}

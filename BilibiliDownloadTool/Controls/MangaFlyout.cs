using BilibiliDownloadTool.Core.Manga;
using BilibiliDownloadTool.Download;
using BilibiliDownloadTool.Pages.ResultPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace BilibiliDownloadTool.Controls
{
    public class MangaFlyout : Flyout
    {
        public MangaFlyout(BiliManga manga, int mcid, string title, BiliMangaMasterResultPage page)
        {
            var control = new MangaFlyoutControl(manga, mcid, title, this, page)
            {
                CanDownload = !manga.IsLocked,
                Title = "提示"
            };
            if (control.CanDownload) control.Info = $"该漫画可下载";
            else control.Info = "该漫画不可下载，请先购买";

            Content = control;
        }
    }
}

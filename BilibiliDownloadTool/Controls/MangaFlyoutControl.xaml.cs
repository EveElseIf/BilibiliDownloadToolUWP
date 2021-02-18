using BilibiliDownloadTool.Core.Manga;
using BilibiliDownloadTool.Pages.ResultPages;
using BilibiliDownloadTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BilibiliDownloadTool.Controls
{
    public sealed partial class MangaFlyoutControl : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="manga"></param>
        /// <param name="mcid"></param>
        /// <param name="title">这个是BiliMangaMaster的标题</param>
        public MangaFlyoutControl(BiliManga manga, int mcid, string title, MangaFlyout parent, BiliMangaMasterResultPage page)
        {
            this.InitializeComponent();
            _manga = manga;
            _mcid = mcid;
            _title = title;
            _parent = parent;
            _page = page;
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MangaFlyoutControl), new PropertyMetadata(null));



        public string Info
        {
            get { return (string)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Info.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(string), typeof(MangaFlyoutControl), new PropertyMetadata(null));



        public bool CanDownload
        {
            get { return (bool)GetValue(CanDownloadProperty); }
            set { SetValue(CanDownloadProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanDownload.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanDownloadProperty =
            DependencyProperty.Register("CanDownload", typeof(bool), typeof(MangaFlyoutControl), new PropertyMetadata(false));

        private readonly BiliManga _manga;
        private readonly int _mcid;
        private readonly string _title;
        private readonly MangaFlyout _parent;
        private readonly BiliMangaMasterResultPage _page;

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _parent.Hide();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _parent.Hide();
            _page.ShowTipWithMessage($"{_title}-{_manga.TitleToString()},epid={_manga.Epid}");
            await MangaDownloadManager.CreateMangaDownloadAsync(_manga, _mcid, _title);
        }        
    }
}

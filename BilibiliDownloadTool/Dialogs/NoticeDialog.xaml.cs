using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BilibiliDownloadTool.Dialogs
{
    public sealed partial class NoticeDialog : ContentDialog
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="primaryText"></param>
        /// <param name="secondaryText">为空时不显示第二按钮</param>
        public NoticeDialog(string title, string content, string primaryText = "确定", string secondaryText = null)
        {
            this.InitializeComponent();
            Title = title;
            ContentTextBlock.Text = content;
            PrimaryButtonText = primaryText;
            if (!string.IsNullOrWhiteSpace(secondaryText)) SecondaryButtonText = secondaryText;
        }
    }
}

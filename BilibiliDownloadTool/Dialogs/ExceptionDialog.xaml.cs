using System;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BilibiliDownloadTool.Dialogs
{
    public sealed partial class ExceptionDialog : ContentDialog
    {
        public ExceptionDialog(Exception ex)
        {
            this.InitializeComponent();
            MsgTextBlock.Text = ex.Message + "\n\n" + ex.StackTrace;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}

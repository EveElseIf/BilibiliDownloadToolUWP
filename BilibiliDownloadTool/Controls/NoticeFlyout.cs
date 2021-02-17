using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace BilibiliDownloadTool.Controls
{
    public class NoticeFlyout : Flyout
    {
        public NoticeFlyout(string title,string content)
        {
            Content = new NoticeFlyoutControl()
            {
                Title = title,
                Text = content
            };
        }
    }
}

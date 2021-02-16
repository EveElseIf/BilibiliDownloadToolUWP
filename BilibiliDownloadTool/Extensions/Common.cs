using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Extensions
{
    public static class Common
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> ts) 
            => new ObservableCollection<T>(ts);
    }
}

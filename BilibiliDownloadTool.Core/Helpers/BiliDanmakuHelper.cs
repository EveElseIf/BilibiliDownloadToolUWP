using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Core.Helpers
{
    public class BiliDanmakuHelper
    {
        private const string GetDanmakuXmlApi = "https://api.bilibili.com/x/v1/dm/list.so";
        public static async Task<string> GetDanmakuXmlStringAsync(int cid)
        {
            if (cid == 0) throw new ArgumentException($"cid=0", nameof(cid));
            var url = GetDanmakuXmlApi + $"?oid=${cid}";
            return await HttpHelper.GetStringAsync(url);
        }
    }
}

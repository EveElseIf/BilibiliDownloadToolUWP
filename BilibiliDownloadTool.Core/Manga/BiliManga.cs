using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliDownloadTool.Core.Manga
{
    public struct BiliManga
    {
        public int Epid { get; set; }
        public string CoverUrl { get; set; }
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public bool IsLocked { get; set; }
        public float Order { get; set; }
        public string TitleToString() => $"{ShortTitle} {Title}";
    }
}

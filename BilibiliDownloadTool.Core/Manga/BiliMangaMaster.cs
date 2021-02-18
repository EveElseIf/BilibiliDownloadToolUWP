using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliDownloadTool.Core.Manga
{
    public struct BiliMangaMaster : IBiliMaster
    {
        public int Mcid { get; set; }
        public string Title { get; set; }
        public string Evaluate { get; set; }
        public string HorizontalCoverUrl { get; set; }
        public string VerticalCoverUrl { get; set; }
        public string SquareCoverUrl { get; set; }
        public List<BiliManga> EpList { get; set; }
    }
}

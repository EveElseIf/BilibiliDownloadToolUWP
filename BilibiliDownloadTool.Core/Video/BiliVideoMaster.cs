namespace BilibiliDownloadTool.Core.Video
{
    public struct BiliVideoMaster : IBiliMaster
    {
        public string Bv { get; set; }
        public long Av { get; set; }
        public int VideoCount { get; set; }
        public string CoverUrl { get; set; }
        public string Title { get; set; }
        public BiliVideo[] Videos { get; set; }
    }
}

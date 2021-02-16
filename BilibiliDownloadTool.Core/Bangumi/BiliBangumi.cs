using BilibiliDownloadTool.Core.Video;

namespace BilibiliDownloadTool.Core.Bangumi
{
    public struct BiliBangumi
    {
        public int EpisodeId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string CoverUrl { get; set; }
        public BiliVideo Video { get; set; }
    }
}

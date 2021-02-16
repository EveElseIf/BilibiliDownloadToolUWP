using BilibiliDownloadTool.Core.Video;

namespace BilibiliDownloadTool.Core.Fav
{
    public struct BiliFav
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int VideoMasterCount { get; set; }
        public BiliVideoMaster[] VideoMasters { get; set; }
    }
}

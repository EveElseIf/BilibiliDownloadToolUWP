namespace BilibiliDownloadTool
{
    public struct BiliUser
    {
        public string Name { get; set; }
        public long Uid { get; set; }
        public string AvatarUrl { get; set; }
        public VipStatus VipStatus { get; set; }
    }

    public enum VipStatus
    {
        IsNotVip,
        IsVip
    }
}

namespace BilibiliDownloadTool.Core.Video
{
    public struct BiliVideoDashInfo
    {
        public BiliVideoQuality[] VideoQualities { get; set; }
        public DashVideoInfo[] Videos { get; set; }
        public DashAudioInfo[] Audios { get; set; }
    }
    public struct DashVideoInfo
    {
        public BiliVideoQuality Quality { get; set; }
        public BiliVideoCodec Codec { get; set; }
        public string Url { get; set; }
    }
    public struct DashAudioInfo
    {
        public BiliAudioQuality Quality { get; set; }
        public BiliAudioCodec Codec { get; set; }
        public string Url { get; set; }
    }
}

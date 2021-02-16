using System.Linq;

namespace BilibiliDownloadTool.Core.Video
{
    public static class Utils
    {
        public static string ToQualityString(this BiliVideoQuality quality)
        {
            switch (quality)
            {
                case BiliVideoQuality.Q240P:
                    return "240P急速";
                case BiliVideoQuality.Q360P:
                    return "360P流畅";
                case BiliVideoQuality.Q480P:
                    return "480P清晰";
                case BiliVideoQuality.Q720P:
                    return "720P高清";
                case BiliVideoQuality.Q720P60:
                    return "720P60高清";
                case BiliVideoQuality.Q1080P:
                    return "1080P高清";
                case BiliVideoQuality.Q1080PPlus:
                    return "1080P+高清";
                case BiliVideoQuality.Q1080P60:
                    return "1080P60高清";
                case BiliVideoQuality.Q4K:
                    return "4K超清";
                default:
                    return "";
            }
        }
        public static DashVideoInfo GetHighestVideoQuality(this BiliVideoDashInfo info)
        {
            return info.Videos.OrderByDescending(v => v.Quality).FirstOrDefault();
        }
        public static DashAudioInfo GetHighestAudioQuality(this BiliVideoDashInfo info)
        {
            return info.Audios.OrderByDescending(a => a.Quality).FirstOrDefault();
        }
        public static DashVideoInfo TryGetTargetQuality(this BiliVideoDashInfo info, BiliVideoQuality quality)
        {
            if (info.VideoQualities.Contains(quality))
                return info.Videos.Where(v => v.Quality == quality).First();
            else return info.GetHighestVideoQuality();
        }
    }
}

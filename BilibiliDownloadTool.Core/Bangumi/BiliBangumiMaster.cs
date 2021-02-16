namespace BilibiliDownloadTool.Core.Bangumi
{
    public struct BiliBangumiMaster : IBiliMaster
    {
        public string Title { get; set; }
        public string Evaluate { get; set; }
        public string CoverUrl { get; set; }
        public string BGCoverUrl { get; set; }
        public string SquareCoverUrl { get; set; }
        public int SeasonId { get; set; }
        public long MediaId { get; set; }
        public MediaType MediaType { get; set; }
        public BiliBangumi[] EpisodeList { get; set; }
    }
    public enum MediaType
    {
        Bangumi = 1,
        Movie,
        Documentary,
        ChineseBangumi,
        TVDrama,
        VarietyShow
    }
}

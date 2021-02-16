using System.Text;

namespace BilibiliDownloadTool.Core.Video
{
    public struct BiliVideo
    {
        public string Bv { get; set; }
        public long Av { get; set; }
        public int Cid { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }

        public override string ToString() => new StringBuilder().AppendLine($"标题：{Title}")
                .AppendLine($"视频名称：{Name}")
                .AppendLine($"分P序号：{Order}")
                .AppendLine($"bv：{Bv}")
                .AppendLine($"av：{Av}")
                .AppendLine($"cid：{Cid}")
                .ToString();
    }
}

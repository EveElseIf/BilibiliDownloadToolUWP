using BilibiliDownloadTool.Core.Bangumi;
using BilibiliDownloadTool.Core.Exceptions;
using BilibiliDownloadTool.Core.Video;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Core.Helpers
{
    public class BiliBangumiHelper
    {
        private const string GetBangumiApi = "https://api.bilibili.com/pgc/view/web/season";
        public static async Task<BiliBangumiMaster> GetBangumiMasterAsync(long ss, string sessdata)
        {
            return await GetBangumiMasterAsync(GetBangumiApi + $"?season_id={ss}", sessdata);
        }
        public static async Task<BiliBangumiMaster> GetBangumiMasterAsync(long ep, string sessdata, object test)
        {
            return await GetBangumiMasterAsync(GetBangumiApi + $"?ep_id={ep}", sessdata);
        }
        private static async Task<BiliBangumiMaster> GetBangumiMasterAsync(string url, string sessdata)
        {
            var json = await HttpHelper.GetJsonAsync(url, sessdata);
            if (json.code != 0) throw new ParsingVideoException();

            var list = new List<BiliBangumi>();
            int i = 1;
            foreach (var b in json.result.episodes)
            {
                var video = new BiliVideo()
                {
                    Av = b.aid,
                    Bv = b.bvid,
                    Cid = b.cid,
                    Title = json.result.season_title,
                    Name = b.long_title,
                    Order = i
                };
                var bangumi = new BiliBangumi()
                {
                    EpisodeId = b.id,
                    Title = video.Title,
                    Name = video.Name,
                    Order = i,
                    CoverUrl = b.cover,
                    Video = video
                };
                list.Add(bangumi);
                i++;
            }
            var master = new BiliBangumiMaster()
            {
                SeasonId = json.result.season_id,
                MediaId = json.result.media_id,
                Title = json.result.season_title,
                Evaluate = json.result.evaluate,
                CoverUrl = json.result.cover,
                BGCoverUrl = json.result.bkg_cover,
                SquareCoverUrl = json.result.square_cover,
                MediaType = (MediaType)(int)json.result.type,
                EpisodeList = list.ToArray()
            };
            return master;
        }
    }
}

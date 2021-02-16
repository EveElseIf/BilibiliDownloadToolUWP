using BilibiliDownloadTool.Core.Bangumi;
using BilibiliDownloadTool.Core.Fav;
using BilibiliDownloadTool.Core.Video;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Core.Helpers
{
    public class BiliFavHelper
    {
        public static async Task<List<BiliFav>> GetUserFavListInfoAsync(long uid, string sessdata)
        {
            var json = await HttpHelper.GetJsonAsync("https://api.bilibili.com/x/v3/fav/folder/created/list-all", sessdata, $"up_mid={uid}&jsonp=jsonp");
            var list = new List<BiliFav>();
            foreach (var item in json.data.list)
            {
                list.Add(new BiliFav()
                {
                    Id = item.id,
                    Title = item.title
                });
            }
            return list;
        }
        public static async Task<BiliFav> GetBiliFavAsync(int id, int pn, string sessdata)
        {
            var json = await HttpHelper.GetJsonAsync("https://api.bilibili.com/x/v3/fav/resource/list", sessdata, $"media_id={id}&pn={pn}&ps=20&order=time");
            if (json.data.medias == null) return default;
            var fav = new BiliFav()
            {
                Id = id,
                Title = json.data.info.title,
                VideoMasterCount = json.data.info.media_count,
            };
            var list = new List<BiliVideoMaster>();
            foreach (var item in json.data.medias)
            {
                list.Add(new BiliVideoMaster()
                {
                    Bv = item.bvid,
                    CoverUrl = item.cover,
                    Title = item.title
                });
            }
            fav.VideoMasters = list.ToArray();
            return fav;
        }
        public static async Task<List<BiliBangumiMaster>> GetFavBangumiMasterListAsync(int page, long uid, string sessdata)
        {
            var json = await HttpHelper.GetJsonAsync("https://api.bilibili.com/x/space/bangumi/follow/list", sessdata, $"type=1&pn={page}&ps=15&vmid={uid}");
            var list = new List<BiliBangumiMaster>();
            if (!(json.data.list.Count > 0)) return null;
            foreach (var bangumi in json.data.list)
            {
                list.Add(await BiliBangumiHelper.GetBangumiMasterAsync((long)bangumi.season_id, sessdata));
            }
            return list;
        }
    }
}

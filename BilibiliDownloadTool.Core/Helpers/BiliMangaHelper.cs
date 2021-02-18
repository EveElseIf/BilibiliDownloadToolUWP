using BilibiliDownloadTool.Core.Exceptions;
using BilibiliDownloadTool.Core.Manga;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Core.Helpers
{
    public class BiliMangaHelper
    {
        private const string GetMangaApi = "https://manga.bilibili.com/twirp/comic.v1.Comic/ComicDetail?device=pc&platform=web";
        private const string GetMangaIndexApi = "https://manga.bilibili.com/twirp/comic.v1.Comic/GetImageIndex?device=pc&platform=web";
        private const string GetMangaTokenApi = "https://manga.bilibili.com/twirp/comic.v1.Comic/ImageToken?device=pc&platform=web";
        public static async Task<BiliMangaMaster> GetMangaMasterAsync(long mc, string sessdata = null)
        {
            var json = await HttpHelper.PostJsonAsync(GetMangaApi, JsonConvert.SerializeObject(new { comic_id = mc }), sessdata);
            if (json.code != 0) throw new MangaNotFoundException("找不到该漫画");
            var master = new BiliMangaMaster()
            {
                Mcid = json.data.id,
                Title = json.data.title,
                Evaluate = json.data.evaluate,
                HorizontalCoverUrl = json.data.horizontal_cover,
                VerticalCoverUrl = json.data.vertical_cover,
                SquareCoverUrl = json.data.square_cover,
                EpList = new List<BiliManga>()
            };
            foreach (var ep in json.data.ep_list)
            {
                master.EpList.Add(new BiliManga()
                {
                    Epid = ep.id,
                    Title = ep.title,
                    ShortTitle = ep.short_title,
                    CoverUrl = ep.cover,
                    IsLocked = ep.is_locked,
                    Order = ep.ord
                });
            }
            master.EpList = master.EpList.OrderBy(m => m.Order).ToList();
            return master;
        }
        public static async Task<List<string>> GetDownloadUrlsAsync(BiliManga manga,int mcid,string sessdata)
        {
            var json1 = await HttpHelper.PostJsonAsync(GetMangaIndexApi, JsonConvert.SerializeObject(new { ep_id = manga.Epid }), sessdata);
            if (json1.code == 1) throw new MangaNeedBuyException("漫画需要购买");
            string url = json1.data.host + json1.data.path;
            var stream = await HttpHelper.GetStreamAsync(url, sessdata);
            using var data = new MemoryStream();
            await stream.CopyToAsync(data);
            stream.Dispose();
            var key = new int[]
                {
                manga.Epid&0xff,
                manga.Epid>>8&0xff,
                manga.Epid>>16&0xff,
                manga.Epid>>24&0xff,
                mcid&0xff,
                mcid>>8&0xff,
                mcid>>16&0xff,
                mcid>>24&0xff
                };
            var bytes = data.ToArray().Skip(9).ToArray();
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= (byte)key[i % 8];
            }
            using var zip = new ZipArchive(new MemoryStream(bytes));
            var json2 = await new StreamReader(zip.GetEntry("index.dat").Open()).ReadToEndAsync();
            var picList = JsonConvert.DeserializeObject<dynamic>(json2).pics;
            var picList2 = new List<string>();
            foreach (var item in picList)
            {
                picList2.Add($"\"{item}\"");
            }
            var payload = JsonConvert.SerializeObject(new { urls = $"[{string.Join(",", picList2)}]" });
            var json = await HttpHelper.PostJsonAsync(GetMangaTokenApi, payload, sessdata);
            if (json.code != 0) throw new Exception("解析失败");
            var picUrlList = new List<string>();
            foreach (var item in json.data)
            {
                picUrlList.Add($"{item.url}?token={item.token}");
            }
            return picUrlList;
        }
    }
}

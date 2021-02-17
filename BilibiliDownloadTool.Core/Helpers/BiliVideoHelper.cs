using BilibiliDownloadTool.Core.Exceptions;
using BilibiliDownloadTool.Core.Video;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Core.Helpers
{
    public static class BiliVideoHelper
    {
        private const string VIDEO_INFO_API = "http://api.bilibili.com/x/web-interface/view";
        private const string VIDEO_GET_PLAY_URL = "http://api.bilibili.com/x/player/playurl";
        /// <summary>
        /// 使用bv获取视频信息
        /// </summary>
        /// <param name="bv"></param>
        /// <param name="sessdata"></param>
        /// <returns></returns>
        public static async Task<BiliVideoMaster> GetVideoMasterAsync(string bv, string sessdata = null)
        {
            if (string.IsNullOrEmpty(bv)) throw new ArgumentException("bv为空");
            var json = await HttpHelper.GetJsonAsync(VIDEO_INFO_API, sessdata, $"bvid={bv}");
            if (json.code == -403) throw new UnauthorizedAccessException("无权限访问该视频");
            if (json.code == -404) throw new VideoNotFoundException("视频不存在");
            if (json.code == -400) throw new ArgumentException("请求错误，请检查参数是否合法");
            if (json.code == 62002) throw new UnauthorizedAccessException("稿件不可见");
            if (json.code != 0) throw new ParsingVideoException();
            return GetVideoMaster(json);
        }
        /// <summary>
        /// 使用av获取视频信息
        /// </summary>
        /// <param name="av"></param>
        /// <param name="sessdata"></param>
        /// <returns></returns>
        public static async Task<BiliVideoMaster> GetVideoMasterAsync(long av, string sessdata = null)
        {
            if (av == 0) throw new ArgumentException("av不合法");
            var json = await HttpHelper.GetJsonAsync(VIDEO_INFO_API, sessdata, $"aid={av}");
            if (json.code == -403) throw new UnauthorizedAccessException("无权限访问该视频");
            if (json.code == -404) throw new VideoNotFoundException("视频不存在");
            if (json.code == -400) throw new ArgumentException("请求错误，请检查参数是否合法");
            if (json.code == 62002) throw new UnauthorizedAccessException("稿件不可见");
            if (json.code != 0) throw new ParsingVideoException();
            return GetVideoMaster(json);
        }
        private static BiliVideoMaster GetVideoMaster(dynamic json)
        {
            var master = new BiliVideoMaster()
            {
                Av = json.data.aid,
                Bv = json.data.bvid,
                Title = json.data.title,
                CoverUrl = json.data.pic,
                VideoCount = json.data.videos
            };
            var list = new List<BiliVideo>();
            foreach (var video in json.data.pages)
            {
                list.Add(new BiliVideo()
                {
                    Av = json.data.aid,
                    Bv = json.data.bvid,
                    Cid = video.cid,
                    Name = video.part,
                    Title = json.data.title,
                    Order = video.page
                });
            }
            master.Videos = list.ToArray();
            return master;
        }
        public static async Task<BiliVideoDashInfo> GetVideoDashStreamAsync(BiliVideo video, string sessdata = null)
        {
            var info = new BiliVideoDashInfo();
            var json = await HttpHelper.GetJsonAsync(VIDEO_GET_PLAY_URL, sessdata, $"bvid={video.Bv}&cid={video.Cid}&fnval=16&fourk=1");
            var list = new List<BiliVideoQuality>();
            foreach (var q in json.data.accept_quality)
            {
                list.Add((BiliVideoQuality)q);
            }
            info.VideoQualities = list.ToArray();
            var list2 = new List<DashVideoInfo>();
            foreach (var v in json.data.dash.video)
            {
                list2.Add(new DashVideoInfo
                {
                    Quality = (BiliVideoQuality)v.id,
                    Codec = ((string)v.codecs).Contains("avc") ? BiliVideoCodec.AVC : BiliVideoCodec.HEVC,
                    Url = v.baseUrl
                });
            }
            info.Videos = list2.ToArray();
            var list3 = new List<DashAudioInfo>();
            foreach (var a in json.data.dash.audio)
            {
                list3.Add(new DashAudioInfo
                {
                    Quality = (BiliAudioQuality)a.id,
                    Codec = BiliAudioCodec.MP4,
                    Url = a.baseUrl
                });
            }
            info.Audios = list3.ToArray();
            return info;
        }
    }
}

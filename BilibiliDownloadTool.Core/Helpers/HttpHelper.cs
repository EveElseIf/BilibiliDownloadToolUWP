using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Core.Helpers
{
    internal static class HttpHelper
    {
        private static readonly HttpClient client = new Func<HttpClient>(() =>
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Add("referer", "http://www.bilibili.com");
            c.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.67 Safari/537.36 Edg/87.0.664.47");
            return c;
        }).Invoke();

        private static string _lastSESSDATA;
        /// <summary>
        /// 获取动态json
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sessdata"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static async Task<dynamic> GetJsonAsync(string url, string sessdata = null, string queryString = null)
        {
            var content = await GetStringAsync(url, sessdata, queryString);
            return JsonConvert.DeserializeObject<dynamic>(content);
        }
        /// <summary>
        /// HttpGet获取字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sessdata"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(string url, string sessdata = null, string queryString = null)
        {
            CheckSessdata(sessdata);
            if (!string.IsNullOrWhiteSpace(queryString)) url += "?" + queryString;
            return await client.GetStringAsync(url);
        }
        private static void CheckSessdata(string sessdata)
        {
            if (_lastSESSDATA != sessdata && sessdata != null) client.DefaultRequestHeaders.Add("Cookie", $"SESSDATA={sessdata}");
            _lastSESSDATA = sessdata;
        }
    }
}

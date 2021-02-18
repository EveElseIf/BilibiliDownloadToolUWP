using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliDownloadTool.Core.Helpers
{
    internal static class HttpHelper
    {
        private static readonly HttpClient _client = new Func<HttpClient>(() =>
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
            return await _client.GetStringAsync(url);
        }
        public static async Task<Stream> GetStreamAsync(string url,string sessdata = null)
        {
            CheckSessdata(sessdata);
            return await _client.GetStreamAsync(url);
        }
        private static void CheckSessdata(string sessdata)
        {
            if (_lastSESSDATA != sessdata && sessdata != null) _client.DefaultRequestHeaders.Add("Cookie", $"SESSDATA={sessdata}");
            _lastSESSDATA = sessdata;
        }
        public static async Task<dynamic> PostJsonAsync(string url,string jsonContent,string sessdata = null)
        {
            CheckSessdata(sessdata);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync(url, content);
            return JsonConvert.DeserializeObject<dynamic>(await resp.Content.ReadAsStringAsync());
        }
    }
}

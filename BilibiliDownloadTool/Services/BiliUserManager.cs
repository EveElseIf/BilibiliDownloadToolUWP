using BilibiliDownloadTool.Dialogs;
using Newtonsoft.Json;
using NLog;
using QRCoder;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace BilibiliDownloadTool.Services
{
    public class BiliUserManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private const string GetUserApi = "https://api.bilibili.com/x/space/myinfo";
        public static async Task<bool> CheckSESSDATAStatusAsync(string sessdata)
        {
            if (string.IsNullOrWhiteSpace(sessdata)) return false;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("cookie", $"SESSDATA={sessdata}");
            try
            {
                var resp = await client.GetStringAsync("https://api.bilibili.com/x/space/myinfo");
                var json = JsonConvert.DeserializeObject<dynamic>(resp);
                if (json.code == 0)
                {
                    Settings.Uid = json.data.mid;
                    _logger.Info("SESSDATA检测结果：有效");
                    return true;
                }
                else
                {
                    _logger.Info($"SESSDATA已失效,值为{sessdata}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                var dialog = new ExceptionDialog(ex);
                await dialog.ShowAsync();
                return false;
            }
        }
        public static async Task<string[]> LoginWithQRCodeAsync(Image qrcodeImage, CancellationToken token)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36 Edg/84.0.522.63");
                var json = JsonConvert.DeserializeObject<dynamic>(await client.GetStringAsync("https://passport.bilibili.com/qrcode/getLoginUrl"));
                string qrcodeUrl = json.data.url;
                string key = json.data.oauthKey;

                static async Task<BitmapImage> GetQRCodeAsync(string content)
                {
                    using var generator = new QRCodeGenerator();
                    using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                    using var qrcode = new PngByteQRCode(data);
                    var imgBytes = qrcode.GetGraphic(20);
                    var img = new BitmapImage();
                    await img.SetSourceAsync(new MemoryStream(imgBytes).AsRandomAccessStream());
                    return img;
                }
                var qrcode = await GetQRCodeAsync(qrcodeUrl);

                await qrcodeImage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                 {
                     qrcodeImage.Source = qrcode;
                 });
                while (true)
                {
                    if (token.IsCancellationRequested) return null;
                    var postResp = await client.PostAsync
                        ("https://passport.bilibili.com/qrcode/getLoginInfo",
                        new StringContent($"oauthKey={key}", Encoding.UTF8, "application/x-www-form-urlencoded"));

                    if (postResp.StatusCode == HttpStatusCode.PreconditionFailed)
                        throw new WebException("HTTP状态码412，已被bilibili暂时封禁IP地址，请稍后再试");

                    var content = await postResp.Content.ReadAsStringAsync();
                    var json2 = JsonConvert.DeserializeObject<dynamic>(content);
                    int.TryParse(json2.data.ToString(), out int resultCode);
                    if (resultCode == -2) return null;
                    if (json2.status == true)
                    {
                        var cookies = postResp.Headers.GetValues("Set-Cookie");
                        var sessdata = Regex.Match(cookies.Where(c => c.Contains("SESSDATA")).FirstOrDefault(), "(?<=SESSDATA=)[%|a-z|A-Z|0-9|*]*")?.Value;
                        var uid = Regex.Match(cookies.Where(c => c.Contains("DedeUserID")).FirstOrDefault(), "(?<=DedeUserID=)[0-9]*")?.Value;
                        _logger.Info($"使用了二维码登录,用户uid为{uid}");
                        return new string[] { sessdata, uid };
                    }
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }
        public static async Task<BiliUser> GetBiliUserAsync(string sessdata)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("cookie", $"SESSDATA={sessdata}");
            try
            {
                var json = JsonConvert.DeserializeObject<dynamic>(await client.GetStringAsync(GetUserApi));
                if (json.code != 0) throw new ArgumentException("无效SESSDATA", nameof(sessdata));
                var user = new BiliUser()
                {
                    Name = json.data.name,
                    Uid = json.data.mid,
                    AvatarUrl = json.data.face,
                    VipStatus = (VipStatus)(int)json.data.vip.status
                };
                _logger.Info($"获取用户信息成功");
                return user;
            }
            catch (ArgumentException ex)
            {
                _logger.Info(ex, "SESSDATA已失效");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}

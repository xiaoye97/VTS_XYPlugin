using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VTS_XYPlugin
{
    public static class NetHelper
    {
        /// <summary>
        /// Get 请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="Timeout"></param>
        /// <returns></returns>
        public static string GetHttpResponse(string url, int Timeout)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = Timeout;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        /// <summary>
        /// 获取可以的端口
        /// </summary>
        public static int GetAvailablePort(string ip)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            TcpListener listener = new TcpListener(ipAddress, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// 下载头像
        /// </summary>
        /// <param name="url">已经去掉http://的链接</param>
        public static void DownloadHead(string url, bool overrideFile = false)
        {
            try
            {
                string savePath = $"{XYPaths.BiliHeadDirPath}/{url.RemoveHttp()}";
                FileInfo file = new FileInfo(savePath);
                if (file.Exists)
                {
                    if (!overrideFile)
                    {
                        //XYLog.LogMessage($"文件已经存在，跳过");
                        return;
                    }
                }
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                }
                XYLog.LogMessage($"开始下载头像 {url}");
                using (var web = new WebClient())
                {
                    web.DownloadFile($"{url}", savePath);
                }
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static string RemoveHttp(this string str)
        {
            return str.Replace("http://", "").Replace("https://", "");
        }
    }
}
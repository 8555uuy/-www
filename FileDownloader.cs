using System;
using System.IO;
using System.Net.Http;

// 【文件下载器：负责把网上一堆文件下到本地，自动建目录】
public class FileDownloader
{
    // 共用一个 HttpClient（表现好，连接可重用）
    private static HttpClient httpClient = new HttpClient();

    // 方法：下载单个文件。参数：下载地址、存到本地的完整路径
    public static void DownloadFile(string url, string 目标路径)
    {
        // 下载前，先把文件所在的目录建好（一次性建全多级子目录）
        string 所在文件夹 = Path.GetDirectoryName(目标路径) ?? "";
        Directory.CreateDirectory(所在文件夹);

        // 先发请求拿响应，检查状态码：不是 200 就跳过，不往下写文件（避免 404 崩整个流程）
        HttpResponseMessage 响应 = httpClient.GetAsync(url).Result;
        if (响应.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine("[下载失败或不存在] " + url);
            return;
        }
        byte[] bytes = 响应.Content.ReadAsByteArrayAsync().Result;
        File.WriteAllBytes(目标路径, bytes);
    }
}
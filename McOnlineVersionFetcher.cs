using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;

// 【在线版本拉取器：联网拉 Mojang 官方清单，只留正式版 release，并解析版本详情】
public class McOnlineVersionFetcher
{
    // 官方版本清单地址（只在这里维护，不在别处抄）
    private const string 清单地址 = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";

    // 方法：拉全部版本清单，只留 type=release 的正式版
    public List<OnlineVersionInfo> FetchReleaseVersions()
    {
        List<OnlineVersionInfo> 结果 = new List<OnlineVersionInfo>();
        HttpClient client = new HttpClient();
        string json = client.GetStringAsync(清单地址).Result;
        JsonDocument doc = JsonDocument.Parse(json);
        JsonElement 版本节点 = doc.RootElement.GetProperty("versions");
        foreach (JsonElement v in 版本节点.EnumerateArray())
        {
            string 类型 = v.GetProperty("type").GetString() ?? "";
            if (类型 != "release")
                continue;
            结果.Add(new OnlineVersionInfo
            {
                Id = v.GetProperty("id").GetString() ?? "",
                Url = v.GetProperty("url").GetString() ?? ""
            });
        }
        return 结果;
    }

    // 方法：拉某个版本的详情 json 原文字符串（供 Program 保存到本地）
    public string FetchVersionDetailRaw(string 详情地址)
    {
        HttpClient client = new HttpClient();
        return client.GetStringAsync(详情地址).Result;
    }

    // 方法：读取本地那份版本详情 json，抽出 client.jar 地址和 libraries 需下载信息
    public VersionDetailInfo LoadVersionDetailFromFile(string json本地路径)
    {
        string json = File.ReadAllText(json本地路径);
        VersionDetailInfo 详情 = new VersionDetailInfo();
        JsonDocument doc = JsonDocument.Parse(json);
        JsonElement 根 = doc.RootElement;

        // client.jar 在 downloads.client.url
        if (根.TryGetProperty("downloads", out JsonElement dl) &&
            dl.TryGetProperty("client", out JsonElement c) &&
            c.TryGetProperty("url", out JsonElement cu))
        {
            详情.ClientJarUrl = cu.GetString() ?? "";
        }

        // 遍历 libraries
        if (根.TryGetProperty("libraries", out JsonElement libs))
        {
            foreach (JsonElement lib in libs.EnumerateArray())
            {
                string url = "";
                string path = "";

                // 顶层 url 和 name
                if (lib.TryGetProperty("url", out JsonElement ue))
                    url = ue.GetString() ?? "";
                if (lib.TryGetProperty("name", out JsonElement ne))
                    path = ne.GetString() ?? "";

                // 若有 natives 且含 Windows 键，优先从 natives.Windows 取 url/path
                if (lib.TryGetProperty("natives", out JsonElement nat))
                {
                    if (nat.TryGetProperty("Windows", out JsonElement winEl))
                    {
                        // winEl 是对象 { url, path }
                        string wUrl = "";
                        if (winEl.TryGetProperty("url", out JsonElement wue))
                            wUrl = wue.GetString() ?? "";
                        string wPath = "";
                        if (winEl.TryGetProperty("path", out JsonElement wpe))
                            wPath = wpe.GetString() ?? "";

                        // Windows 原生库：优先用 url；url 空但有 path 时拼前缀
                        if (!string.IsNullOrEmpty(wUrl))
                        {
                            url = wUrl;
                            if (!string.IsNullOrEmpty(wPath)) path = wPath;
                        }
                        else if (!string.IsNullOrEmpty(wPath))
                        {
                            url = "https://libraries.minecraft.net/" + MavenToUnixPath(wPath);
                            path = wPath;
                        }

                        if (!string.IsNullOrEmpty(url))
                        {
                            详情.Libraries.Add(new LibraryInfo { Url = url, Path = path });
                            continue;
                        }
                    }
                }

                // 没有 url 但有 path（maven 坐标），拼 libraries.minecraft.net 前缀
                if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(path))
                {
                    url = "https://libraries.minecraft.net/" + MavenToUnixPath(path);
                }

                if (!string.IsNullOrEmpty(url))
                {
                    详情.Libraries.Add(new LibraryInfo { Url = url, Path = path });
                }
            }
        }

        return 详情;
    }

    // 把 mavenName 形如 "com.foo:bar:1.0" 转成路径 com/foo/bar/1.0/bar-1.0.jar
    private static string MavenToUnixPath(string mavenName)
    {
        string[] parts = mavenName.Split(new[] { ':' }, 4);
        string groupId = parts[0].Replace('.', '/');
        string artifactId = parts[1];
        string version = parts[2];
        return groupId + "/" + artifactId + "/" + version + "/" + artifactId + "-" + version + ".jar";
    }
}

// 【在线版本信息：清单里的一个 release 条目】
public class OnlineVersionInfo
{
    public string Id = string.Empty;   // 版本编号，如 1.20.4
    public string Url = string.Empty;  // 版本详情 json 地址
}

// 【版本详情信息：要下载的文件集合】
public class VersionDetailInfo
{
    public string ClientJarUrl = string.Empty;  // client.jar 下载地址
    public List<LibraryInfo> Libraries = new List<LibraryInfo>();
}

// 【单个库文件：一个 jar 的下载信息】
public class LibraryInfo
{
    public string Url = string.Empty;  // 下载地址
    public string Path = string.Empty; // maven 坐标/相对路径
}
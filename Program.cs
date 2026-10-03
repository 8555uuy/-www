using System;
using System.Collections.Generic;
using System.IO;

// ========= 主程序（只做控制台交互，逻辑很少） =========
McVersionScanner scanner = new McVersionScanner();
const string savedPathFile = "saved_path.txt";
ShowMainMenu();
// X = 退出或退出失败


void ShowMainMenu()
{
    // 首次进入：在桌面建一个 itsl2 文件夹当版本文件夹基准
    string 桌面路径 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
    string itsl2路径 = 桌面路径 + "\\" + "itsl2";
    Directory.CreateDirectory(itsl2路径);

    while (true)
    {
        Console.Clear();//清除文字
        Console.WriteLine("========== Minecraft 启动器主菜单 ==========");
        Console.WriteLine("  1. 本地导入版本");
        Console.WriteLine("  2. 下载版本");
        Console.WriteLine("  0 或 回车: 退出");
        Console.WriteLine("=".PadRight(41, '='));
        Console.WriteLine();
        Console.WriteLine("  请输入选项：");
        string? option = Console.ReadLine()?.Trim() ?? "";
        /*
         ?.如果是null，那么?.后面的代码就不执行了，反之则相反

        Trim()：去掉字符串前后空格
         
         
         */
        switch (option)
        {
            case "1":
                LocalVersionImport(scanner, savedPathFile);
                break;
            case "2":
                DownloadVersionMenu();
                break;
            case "0":
            case "":
                return;
            default:
                Console.WriteLine("  [提示] 无效输入，请重新选择。");
                Thread.Sleep(800);
                break;
        }

        Console.WriteLine();
        Console.WriteLine("  按回车键返回主菜单...");
        Console.ReadLine();
    }
}

// 本地导入逻辑（输入1的逻辑）
void LocalVersionImport(McVersionScanner scanner, string savedPathFile)
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("请输入版本文件夹路径：（输入 xiaobao = 用上次保存的地址）");
        string? versionPath = Console.ReadLine();

        // 路径为空则直接回到循环开头重新输入
        if (string.IsNullOrWhiteSpace(versionPath))
        {
            Console.WriteLine("路径不能为空，请重新输入。");
            continue;
        }

        // 输入 "xiaobao" = 直接用上次保存的地址
        if (versionPath == "xiaobao")
        {
            if (!File.Exists(savedPathFile))
            {
                Console.WriteLine("X  还没有上次保存的地址，请直接输入完整路径");
                continue;
            }
            string lastPath = File.ReadAllText(savedPathFile);
            if (string.IsNullOrWhiteSpace(lastPath))
            {
                Console.WriteLine("X  上次保存的地址是空的，请直接输入完整路径");
                continue;
            }
            versionPath = lastPath;
            Console.WriteLine($"已自动使用上次保存的地址：{versionPath}");
        }

        // 执行扫描
        var foundVersions = scanner.ScanAllVersions(versionPath);

        Console.WriteLine();
        Console.WriteLine($"共找到 {foundVersions.Count} 个版本：");
        for (int i = 0; i < foundVersions.Count; i++)
        {
            McVersion v = foundVersions[i];
            Console.WriteLine($"{i + 1}  {v.BanBenMing}    [{v.WenJianJiaLuJing}]");
        }

        if (foundVersions.Count > 0)
        {
            // 保存本次路径（只保留这一个结果）
            File.WriteAllText(savedPathFile, versionPath);
            Console.Write("请选择版本（输入编号）：");
            int index = -1;
            bool parseOk = int.TryParse(Console.ReadLine(), out index);
            if (!parseOk || index < 1 || index > foundVersions.Count)
            {
                Console.WriteLine("X  无效编号，返回主菜单");
                return;
            }
            McVersion selectedVersion = foundVersions[index - 1];
            Console.WriteLine();
            Console.WriteLine($"已选择版本：{selectedVersion.BanBenMing}");
            Console.WriteLine($"版本路径：  {selectedVersion.WenJianJiaLuJing}");
            // 版本选择流程结束，回到主菜单
            return;
        }
        else
        {
            Console.WriteLine("X  该版本文件夹不存在或未找到版本，返回主菜单");
            return;
        }
    }
}

// 下载版本界面（真实逻辑：拉清单 -> 选版本 -> 下详情 -> 下client.jar -> 下libraries）
void DownloadVersionMenu()
{
    Console.WriteLine();
    Console.WriteLine("========== 版本在线下载 ==========");

    // 所有下载都存到桌面 itsl2 文件夹下
    string 根目录 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\itsl2";
    Directory.CreateDirectory(根目录);

    McOnlineVersionFetcher fetcher = new McOnlineVersionFetcher();
    List<OnlineVersionInfo> releaseVersions = fetcher.FetchReleaseVersions();

    Console.WriteLine("共拉取到 " + releaseVersions.Count + " 个正式版（release）：");
    for (int i = 0; i < releaseVersions.Count; i++)
    {
        Console.WriteLine("  " + (i + 1) + "  " + releaseVersions[i].Id);
    }

    if (releaseVersions.Count == 0)
    {
        Console.WriteLine("X  没有可用版本，返回主菜单");
        return;
    }

    Console.Write("请选择版本（输入编号）：");
    int 选号 = -1;
    bool 解析成功 = int.TryParse(Console.ReadLine(), out 选号);
    if (!解析成功 || 选号 < 1 || 选号 > releaseVersions.Count)
    {
        Console.WriteLine("X  无效编号，返回主菜单");
        return;
    }
    OnlineVersionInfo 选中版本 = releaseVersions[选号 - 1];
    string 版本id = 选中版本.Id;
    string 版本文件夹 = 根目录 + "/versions/" + 版本id;
    string 版本详情json路径 = 版本文件夹 + "/" + 版本id + ".json";

    // 1. 拉详情 json 原文，存本地
    string 详情json原文 = fetcher.FetchVersionDetailRaw(选中版本.Url);
    Directory.CreateDirectory(版本文件夹);
    File.WriteAllText(版本详情json路径, 详情json原文);
    Console.WriteLine("已保存版本详情： " + 版本详情json路径);

    // 2. 读取本地详情，下 client.jar
    VersionDetailInfo 详情 = fetcher.LoadVersionDetailFromFile(版本详情json路径);
    if (!string.IsNullOrEmpty(详情.ClientJarUrl))
    {
        string clientJar路径 = 版本文件夹 + "/client.jar";
        Console.WriteLine("正在下载 client.jar ...");
        FileDownloader.DownloadFile(详情.ClientJarUrl, clientJar路径);
    }

    // 3. 遍历 libraries，每个 jar 下到 libraries/ 下（按 maven path 建多层文件夹）
    int 库数量 = 详情.Libraries.Count;
    Console.WriteLine("共 " + 库数量 + " 个库需要下载：");
    int 计数 = 0;
    foreach (LibraryInfo 库 in 详情.Libraries)
    {
        计数++;
        // 从 Url 抽 libraries.minecraft.net/ 之后那一段（多级目录+jar文件名），本地也按同样的相对结构存
        string 相对段 = 库.Url;
        int 域名下标 = 库.Url.IndexOf("libraries.minecraft.net/", System.StringComparison.OrdinalIgnoreCase);
        if (域名下标 >= 0)
            相对段 = 库.Url.Substring(域名下标 + "libraries.minecraft.net/".Length);
        else
            相对段 = Path.GetFileName(库.Url);  // 兜底，取不到就用文件名
        string 库目标 = 根目录 + "/libraries/" + 相对段.Replace('\\', '/');
        Console.WriteLine("  [" + 计数 + "/" + 库数量 + "] " + 库.Url + "  ->  " + 库目标);
        FileDownloader.DownloadFile(库.Url, 库目标);
    }

    Console.WriteLine();
    Console.WriteLine("[完成] 版本 " + 版本id + " 下载结束");
}


// 下方类型定义放在所有顶级语句之后，符合 C# 语法
public class McVersion
{
    public string BanBenMing { get; set; } = string.Empty; //版本名称
    public string? WenJianJiaLuJing { get; set; }          //版本文件夹路径
}

// 【工具类：专门做版本扫描，单一职责】
public class McVersionScanner
{
    // 方法：接收根路径，返回找到的所有版本对象列表
    public List<McVersion> ScanAllVersions(string? rootPath)
    {
        List<McVersion> versionList = new();
        // 判断路径与文件夹是否存在
        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            return versionList;
        }
        string[] dirArray = Directory.GetDirectories(rootPath);
        foreach (var dir in dirArray)
        {
            versionList.Add(new McVersion
            {
                BanBenMing = Path.GetFileName(dir),
                WenJianJiaLuJing = dir
            });
        }
        return versionList;
    }
}

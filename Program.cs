using System;
using System.Collections.Generic;

// ========= 主程序（只做控制台交互，逻辑很少） =========
McVersionScanner scanner = new McVersionScanner();
const string savedPathFile = "saved_path.txt";
ShowMainMenu();
// X = 退出或退出失败


void ShowMainMenu()
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("========== Minecraft 启动器主菜单 ==========");
        Console.WriteLine("  1. 本地导入版本");
        Console.WriteLine("  2. 下载版本");
        Console.WriteLine("  0 或 回车: 退出");
        Console.WriteLine("=".PadRight(41, '='));
        Console.WriteLine();
        Console.WriteLine("  请输入选项：");
        string? option = Console.ReadLine()?.Trim() ?? "";

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

// 下载版本界面（占位，等待后续开发）
void DownloadVersionMenu()
{
    Console.WriteLine();
    Console.WriteLine("========== [功能开发中] 版本下载 ==========");
    Console.WriteLine("  此功能尚未实现，敬请期待！");
    Console.WriteLine("=".PadRight(41, '='));
    Console.WriteLine();
    Console.ReadLine();
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

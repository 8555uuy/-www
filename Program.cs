// ========= 主程序（只做控制台交互，逻辑很少） =========
McVersionScanner scanner = new McVersionScanner();
while (true)
{
    Console.WriteLine("请输入版本文件夹路径：");
    string? versionPath = Console.ReadLine();
    // 路径为空则直接回到循环开头重新输入
    if (string.IsNullOrWhiteSpace(versionPath))
    {
        Console.WriteLine("路径不能为空，请重新输入。");
        continue;
    }
    List<McVersion> versionList = scanner.ScanAllVersions(versionPath);
    if (versionList.Count > 0)
    {
        Console.WriteLine("\n===本地版本列表===");
        int idx = 1;
        foreach (var ver in versionList)
        {
            Console.WriteLine($"{idx}. {ver.BanBenMing}");
            idx++;
        }
    }
    else
    {
        Console.WriteLine("X  路径不存在或无效");
    }
    Console.WriteLine("\n指令：（输入数字 = 打开对应文件夹，回车 = 重新扫描，exit = 退出程序）");
    string? input = Console.ReadLine();
    if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
    if (int.TryParse(input, out int num) && num >= 1 && num <= versionList.Count)
    {
        string? folder = versionList[num - 1].WenJianJiaLuJing;
        if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
        else
        {
            Console.WriteLine("X  该版本文件夹不存在");
        }
    }
    //其余任意输入（包括回车）都继续循环，回到顶部重新输入路径扫描
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

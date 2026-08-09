namespace ServerFramework.Logging;

// 框架统一的日志出口：目前只打到控制台
// 以后想接文件、日志库，只改这一个类，游戏层不用动
public static class ServerLog
{
    public static void Info(string message) => Console.WriteLine(message);
    public static void Warn(string message) => Console.WriteLine($"[Warn] {message}");
    public static void Error(string message) => Console.WriteLine($"[Error] {message}");
}

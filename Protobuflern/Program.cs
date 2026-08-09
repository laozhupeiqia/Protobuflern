namespace Protobuflern
{
    // 只负责一件事：启动当前游戏服务器
    // 框架初始化和 Handler 注册都在 GameServer 里
    internal static class Program
    {
        static void Main()
        {
            new GameServer(9001).Run();
        }
    }
}

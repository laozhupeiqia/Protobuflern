using Protobuflern.Demo;
using Protobuflern.Handles;
using ServerFramework.Dispatch;
using ServerFramework.Network;
using ServerFramework.Session;

namespace Protobuflern
{
    internal static class Program
    {
        static void Main()
        {
            const int port = 9001;

            // 1. 框架初始化
            var transport = new NetworkTransport(port);
            var sessions = SessionManager.Create(transport);
            var dispatcher = new MessageDispatcher();

            // 2. 游戏层注册处理器：包类型 -> Handler（加新协议就在这加一行）
            dispatcher.Register((int)PacketType.Hello, new HelloHandler());
            dispatcher.Register((int)PacketType.Login, new LoginHandler());
            dispatcher.Register((int)PacketType.Action1, new Handle1());
            dispatcher.Register((int)PacketType.Action2, new Handle2());
            dispatcher.Register((int)PacketType.TestBroadcast, new BroadcastTestHandler());

            Console.WriteLine($"UDP 服务器已启动，监听端口 {port} ...");

            // 3. 跑主循环（收包/分发/清理都在框架里）
            new NetworkServer(transport, sessions, dispatcher).Run();
        }
    }
}

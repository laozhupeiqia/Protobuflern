using Protobuflern.Demo;
using Protobuflern.Handles;
using ServerFramework.Dispatch;
using ServerFramework.Network;
using ServerFramework.Session;

namespace Protobuflern
{
    // 当前游戏的服务器配置层：职责链 Program -> GameServer -> ServerFramework
    // 这里只管"当前这个游戏"的初始化：框架怎么搭、本游戏有哪些 Handler、怎么启动
    internal class GameServer
    {
        private readonly int port;
        private readonly NetworkServer server;

        public GameServer(int port)
        {
            this.port = port;
            // 1. 框架初始化（网络 / 会话 / 分发）
            var transport = new NetworkTransport(port);
            var sessions = SessionManager.Create(transport);
            var dispatcher = new MessageDispatcher();

            // 2. 注册当前游戏自己的 Handler
            RegisterHandlers(dispatcher);

            // 3. 组装服务器主循环
            server = new NetworkServer(transport, sessions, dispatcher);
        }

        // 当前游戏有哪些 Handler：加新协议就在这加一行
        private void RegisterHandlers(MessageDispatcher dispatcher)
        {
            dispatcher.Register((int)PacketType.Hello, new HelloHandler());
            dispatcher.Register((int)PacketType.Register, new RegisterHandler());
            dispatcher.Register((int)PacketType.Login, new LoginHandler());
            dispatcher.Register((int)PacketType.Heartbeat, new HeartbeatHandler());
            dispatcher.Register((int)PacketType.PlayerFrame, new PlayerFrameHandler());
            dispatcher.Register((int)PacketType.KillEvent, new KillEventHandler());
            dispatcher.Register((int)PacketType.DropEvent, new DropEventHandler());
            dispatcher.Register((int)PacketType.Action1, new Handle1());
            dispatcher.Register((int)PacketType.Action2, new Handle2());
            dispatcher.Register((int)PacketType.TestBroadcast, new BroadcastTestHandler());
        }

        // 启动服务器（阻塞，直到进程退出）
        public void Run()
        {
            Console.WriteLine($"UDP 服务器已启动，监听端口 {port} ...");
            server.Run();
        }
    }
}

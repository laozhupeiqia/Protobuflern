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
            dispatcher.Register((int)PacketType.JoinRoom, new JoinRoomHandler());
            dispatcher.Register((int)PacketType.GetRoomList, new GetRoomListHandler());
            dispatcher.Register((int)PacketType.LeaveRoom, new LeaveRoomHandler());
        }

        // 心跳 5s 一次；超过这个秒数没发包视为客户端已关闭/断网，后台定时清理
        private const int StaleTimeoutSeconds = 30;

        // 启动服务器（阻塞，直到进程退出）
        public void Run()
        {
            Console.WriteLine($"UDP 服务器已启动，监听端口 {port} ...");

            // 客户端直接关闭时 UDP 服务器收不到任何通知，只能靠超时回收：
            // 否则账号/房间会一直残留（在线人数虚高、列表里出现幽灵玩家）
            using var staleTimer = new System.Threading.Timer(
                _ => CleanupStaleSessions(), null,
                TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));

            server.Run();
        }

        // 清理超时会话：踢会话 + 清在线表 + 退出房间（房主走了会自动解散房间）
        private static void CleanupStaleSessions()
        {
            DateTime now = DateTime.UtcNow;
            foreach (ClientSession session in SessionManager.Instance.All.ToList())
            {
                if ((now - session.LastActiveTime).TotalSeconds <= StaleTimeoutSeconds)
                    continue;

                if (session.PlayerId != null)
                {
                    RoomRegistry.RemovePlayerAndNotify(session.PlayerId);
                    PlayerRegistry.Remove(session.PlayerId);
                }
                SessionManager.Instance.Kick(session.RemoteEndPoint);
                Console.WriteLine($"[超时掉线] {session} 超过 {StaleTimeoutSeconds}s 未发包，移除");
            }
        }
    }
}

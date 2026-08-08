using System.Net;

namespace Protobuflern
{
    // 在线客户端管理：UDP 无连接，用"IP:端口 -> 最后活跃时间"表维护在线状态
    // 所有增删都走这里，外面不直接碰字典
    internal static class ClientManager
    {
        private const int ClientTimeoutSeconds = 20;    // 超过这么久没发包就踢
        private const int CleanupIntervalSeconds = 10;  // 间隔多久检查一次
        private static readonly Dictionary<IPEndPoint, DateTime> clients = new();
        private static readonly HashSet<IPEndPoint> loggedIn = new();   // 已登录的客户端（在线 ≠ 已登录）
        private static DateTime lastCleanup = DateTime.UtcNow;

        public static int Count => clients.Count;

        // 广播要遍历的全部在线客户端（只读，防止外面改表）
        public static IReadOnlyCollection<IPEndPoint> All => clients.Keys;

        // 收到一个包：新客户端加入并打日志，老客户端刷新活跃时间
        public static bool AddOrTouch(IPEndPoint remote)
        {
            bool isNew = !clients.ContainsKey(remote);
            clients[remote] = DateTime.UtcNow;
            if (isNew)
                Console.WriteLine($"[新客户端] {remote} 加入，在线 {clients.Count} 人");
            return isNew;
        }

        // 标记为已登录（登录成功时才调用）
        public static void MarkLoggedIn(IPEndPoint remote) => loggedIn.Add(remote);

        // 是否已登录（动作类消息的门禁）
        public static bool IsLoggedIn(IPEndPoint remote) => loggedIn.Contains(remote);

        // 踢出（立即删），返回是否真的删掉了；登录状态一并清除
        public static bool Kick(IPEndPoint remote)
        {
            loggedIn.Remove(remote);
            return clients.Remove(remote);
        }

        // 主循环每轮调它：到点就自动清扫超时者，没到点直接返回
        public static void MaybeCleanup()
        {
            if ((DateTime.UtcNow - lastCleanup).TotalSeconds <= CleanupIntervalSeconds)
                return;
            lastCleanup = DateTime.UtcNow;

            var idle = clients
                .Where(kv => (DateTime.UtcNow - kv.Value).TotalSeconds > ClientTimeoutSeconds)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var ep in idle)
            {
                Console.WriteLine($"[掉线] {ep} 超时未发包，踢出");
                Kick(ep);
            }
        }
    }
}

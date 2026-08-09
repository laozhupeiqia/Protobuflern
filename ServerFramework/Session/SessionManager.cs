using System.Net;
using System.Net.Sockets;
using ServerFramework.Network;

namespace ServerFramework.Session;

// 在线会话管理：UDP 无连接，用"IP:端口 -> 会话"表维护在线状态
// 所有增删都走这里；暴露 static Instance 供游戏层静态工具（如发邮件）直接调用
public sealed class SessionManager
{
    private const int ClientTimeoutSeconds = 20;    // 超过这么久没发包就踢
    private const int CleanupIntervalSeconds = 10;  // 间隔多久检查一次

    private static SessionManager? _instance;
    public static SessionManager Instance
        => _instance ?? throw new InvalidOperationException("SessionManager 尚未初始化，请先调用 Create()");

    private readonly NetworkTransport _transport;
    private readonly Dictionary<IPEndPoint, ClientSession> _sessions = new();
    private long _nextSessionId;
    private DateTime _lastCleanup = DateTime.UtcNow;

    private SessionManager(NetworkTransport transport) => _transport = transport;

    // 初始化并记录单例，返回实例供主循环使用
    public static SessionManager Create(NetworkTransport transport)
    {
        _instance = new SessionManager(transport);
        return _instance;
    }

    public int Count => _sessions.Count;

    // 广播要遍历的全部在线会话（只读，防止外面改表）
    public IReadOnlyCollection<ClientSession> All => _sessions.Values;

    // 收到一个包：新客户端建会话并打日志，老客户端刷新活跃时间
    public ClientSession GetOrAdd(IPEndPoint remote)
    {
        if (_sessions.TryGetValue(remote, out ClientSession? session))
        {
            session.LastActiveTime = DateTime.UtcNow;
            return session;
        }

        session = new ClientSession(_transport, remote, ++_nextSessionId);
        _sessions[remote] = session;
        Console.WriteLine($"[新客户端] {session} 加入，在线 {_sessions.Count} 人");
        return session;
    }

    // 踢出（立即删），返回是否真的删掉了
    public bool Kick(IPEndPoint remote) => _sessions.Remove(remote);

    // 给所有在线会话广播一条消息（游戏层主动调用，如全服邮件）
    public void Broadcast(int messageId, byte[] body)
    {
        byte[] packet = FrameCodec.Encode(messageId, body);
        foreach (ClientSession session in _sessions.Values)
        {
            try
            {
                _transport.SendTo(session.RemoteEndPoint, packet, packet.Length);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[发送失败] {session} 掉线，移除：{ex.Message}");
                _sessions.Remove(session.RemoteEndPoint);
            }
        }
    }

    // 主循环每轮调它：到点就自动清扫超时者，没到点直接返回
    public void MaybeCleanup()
    {
        if ((DateTime.UtcNow - _lastCleanup).TotalSeconds <= CleanupIntervalSeconds)
            return;
        _lastCleanup = DateTime.UtcNow;

        var idle = _sessions
            .Where(kv => (DateTime.UtcNow - kv.Value.LastActiveTime).TotalSeconds > ClientTimeoutSeconds)
            .Select(kv => kv.Key)
            .ToList();

        foreach (IPEndPoint remote in idle)
        {
            Console.WriteLine($"[掉线] {remote} 超时未发包，踢出");
            _sessions.Remove(remote);
        }
    }
}

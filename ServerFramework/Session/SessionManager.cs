using System.Net;
using System.Net.Sockets;
using ServerFramework.Network;

namespace ServerFramework.Session;

// 在线会话管理：UDP 无连接，用"IP:端口 -> 会话"表维护在线状态
// 所有增删都走这里；暴露 static Instance 供游戏层静态工具（如发邮件）直接调用
// 注意：会话不会因为"多久没发包"就被踢——玩家挂机/去厕所/网络波动都不该掉线。
// 失联后的账号复用由登录时判定（旧会话失联→新登录接管），见 LoginHandler
public sealed class SessionManager
{
    private static SessionManager? _instance;
    public static SessionManager Instance
        => _instance ?? throw new InvalidOperationException("SessionManager 尚未初始化，请先调用 Create()");

    private readonly NetworkTransport _transport;
    private readonly Dictionary<IPEndPoint, ClientSession> _sessions = new();
    private long _nextSessionId;

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
    public void Broadcast(int messageId, byte[] body) => Broadcast(null, messageId, body);

    // 广播时跳过某个会话（位置/操作同步要排除发送者自己）
    public void Broadcast(ClientSession? exclude, int messageId, byte[] body)
    {
        byte[] packet = FrameCodec.Encode(messageId, body);
        foreach (ClientSession session in _sessions.Values)
        {
            if (exclude != null && session == exclude)
                continue;
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
}

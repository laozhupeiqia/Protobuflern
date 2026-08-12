using System.Net;
using System.Text;
using Google.Protobuf;
using ServerFramework.Network;

namespace ServerFramework.Session;

// 一个在线客户端的会话：把"网络连接"和"玩家"分开
// 上层 Handler 只碰这个对象发消息，不碰 Socket / IPEndPoint / 组帧细节
public sealed class ClientSession
{
    private readonly NetworkTransport _transport;

    public long SessionId { get; }                 // 会话唯一编号
    public IPEndPoint RemoteEndPoint { get; }      // 网络地址
    public DateTime LastActiveTime { get; internal set; }  // 最后活跃时间，超时清理用
    public bool IsAuthenticated { get; set; }      // 是否已登录（登录门禁由游戏层判断）
    public string? PlayerId { get; set; }          // 登录后绑定的玩家标识（可空：未登录）

    internal ClientSession(NetworkTransport transport, IPEndPoint remoteEndPoint, long sessionId)
    {
        _transport = transport;
        RemoteEndPoint = remoteEndPoint;
        SessionId = sessionId;
        LastActiveTime = DateTime.UtcNow;
    }

    // 给这个客户端发一个消息：内部完成组帧 + 投递
    public void Send(int messageId, byte[] body)
    {
        byte[] packet = FrameCodec.Encode(messageId, body);
        _transport.SendTo(RemoteEndPoint, packet, packet.Length);
    }

    // 发一句 UTF8 文本的便捷方法
    public void Reply(int messageId, string text)
        => Send(messageId, Encoding.UTF8.GetBytes(text));

    // 发一条 protobuf 消息的便捷方法：body 自动序列化
    public void Reply(int messageId, IMessage msg)
        => Send(messageId, msg.ToByteArray());

    public override string ToString() => $"{RemoteEndPoint}(会话{SessionId})";
}

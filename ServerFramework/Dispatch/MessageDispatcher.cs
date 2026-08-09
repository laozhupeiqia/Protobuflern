using ServerFramework.Logging;
using ServerFramework.Network;
using ServerFramework.Session;

namespace ServerFramework.Dispatch;

// 消息分发：MessageId -> 处理器的路由表，按类型找到对应处理器
// 不认识任何具体业务，只做"查表转发"
public sealed class MessageDispatcher
{
    private readonly Dictionary<int, IMessageHandler> _handlers = new();

    // 游戏层注册：dispatcher.Register((int)PacketType.Login, new LoginHandler());
    public void Register(int messageId, IMessageHandler handler)
        => _handlers[messageId] = handler;

    public bool Dispatch(ClientSession session, NetworkMessage msg)
    {
        if (_handlers.TryGetValue(msg.MessageId, out IMessageHandler? handler))
        {
            handler.Handle(session, msg.Body);
            return true;
        }
        ServerLog.Warn($"[Dispatch] 未注册的消息类型 0x{msg.MessageId:X2}，丢弃");
        return false;
    }
}

using Google.Protobuf;
using ServerFramework.Session;

namespace ServerFramework.Dispatch;

// 强类型 Handler 基类：把 body 反序列化成 T 后再调业务 Handle
// 游戏层这样写即可：
//   class LoginHandler : MessageHandler<Player> { public override void Handle(ClientSession s, Player p) {...} }
public abstract class MessageHandler<T> : IMessageHandler where T : IMessage<T>, new()
{
    private static readonly MessageParser<T> Parser = new(() => new T());

    void IMessageHandler.Handle(ClientSession session, byte[] body)
        => Handle(session, Parser.ParseFrom(body));

    public abstract void Handle(ClientSession session, T message);
}

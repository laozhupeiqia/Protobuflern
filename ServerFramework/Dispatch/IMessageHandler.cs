using ServerFramework.Session;

namespace ServerFramework.Dispatch;

// 分发表要存的统一入口（非泛型，这样 Register 才能把任意 T 的处理器收进同一张表）
// 业务代码不要直接实现它，继承 MessageHandler<T> 拿到强类型消息
public interface IMessageHandler
{
    void Handle(ClientSession session, byte[] body);
}

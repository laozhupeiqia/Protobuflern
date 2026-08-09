using Protobuflern.Demo;
using Protobuflern.Interfaces;

namespace Protobuflern.Handles
{
    // 连接测试：客户端发一个空包(0x00)，服务器回一句确认，说明链路已经通了
    // 不做登录门禁——测试连接本来就是登录之前干的事
    internal class HelloHandler : IHandle
    {
        public void Handle(GamePacket pkt)
        {
            Console.WriteLine($"[{pkt.Remote}] 收到连接测试包，回复确认");
            Sender.Reply(pkt, PacketType.ServerReply, "恭喜你，当你收到这条消息的时候说明服务器连接成功了");
        }
    }
}

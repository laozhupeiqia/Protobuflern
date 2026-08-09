using Protobuflern.Demo;
using Protobuflern.Interfaces;

namespace Protobuflern.Handles
{
    // 登录业务：收到登录请求 → 标记已登录 → 回"登录成功"
    internal class LoginHandler : IHandle
    {
        public void Handle(GamePacket pkt)
        {
            // 登录请求的 body 也是 Player，复用同一个协议
            var player = Player.Parser.ParseFrom(pkt.Buffer, pkt.Offset, pkt.Count);

            // 标记已登录：登录成功之前，动作类消息一律被门禁挡下
            ClientManager.MarkLoggedIn(pkt.Remote);
            Console.WriteLine($"[{pkt.Remote}] {player.Name} 登录成功");

            // 回登录成功（类型 0x12），客户端收到这个才算是"进了游戏"
            Sender.Reply(pkt, PacketType.LoginOk, $"登录成功，欢迎 {player.Name}");
        }
    }
}

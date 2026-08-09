using Protobuflern.Demo;
using Protobuflern.Interfaces;

namespace Protobuflern.Handles
{
    internal class Handle2: IHandle
    {
        // 类型 2 的业务：假如 body 不是 Player、而是别的消息，你只在这里解析自己的类型
        public void Handle(GamePacket pkt)
        {
            var player = Player.Parser.ParseFrom(pkt.Buffer, pkt.Offset, pkt.Count);
            Console.WriteLine($"[{pkt.Remote}] {player.Name} 做了事情2");
        }
    }
}

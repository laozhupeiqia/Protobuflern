using Protobuflern.Demo;
using Protobuflern.Interfaces;

namespace Protobuflern.Handles
{
    internal class Handle1: IHandle
    {
        public void Handle(GamePacket pkt)
        {
            var player = Player.Parser.ParseFrom(pkt.Buffer, pkt.Offset, pkt.Count);
            Console.WriteLine($"[{pkt.Remote}] {player.Name} 做了事情1");
            Sender.Reply(pkt, PacketType.ServerReply, "操作完成");
        }
    }
}

using Protobuflern.Demo;

namespace Protobuflern
{
    internal class Handle1
    {
        // 类型 1 的业务：解析自己的消息，做完后回一句
        internal static void HandlePlayerAction1(GamePacket pkt)
        {
            var player = Player.Parser.ParseFrom(pkt.Buffer, pkt.Offset, pkt.Count);
            Console.WriteLine($"[{pkt.Remote}] {player.Name} 做了事情1");
            Responder.Reply(pkt, PacketTypes.ServerReply, "操作完成");
        }
    }
}

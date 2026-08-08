using System.Net;

namespace Protobuflern
{
    // 帧解析 + 分发：校验包头、切出数据体、组装成 GamePacket，再按类型交给对应处理器
    // 只做框架层的事，body 里装的是什么由各处理器自己解析
    internal static class FrameParser
    {
        // 数据包入口：校验失败直接丢弃，成功则按类型分发
        internal static void Process(IPEndPoint remote, byte[] packet, int length)
        {
            // 1. 校验包头（至少 5 字节：2 包头 + 1 类型 + 2 长度）
            if (length < 5 || packet[0] != 0xAC || packet[1] != 0xAC)
            {
                Console.WriteLine($"[{remote}] 包头校验失败，丢弃");
                return;
            }

            // 2. 解帧：数据体长度（大端）+ 边界校验
            int bodyLen = (packet[3] << 8) | packet[4];
            if (length < 5 + bodyLen)
            {
                Console.WriteLine($"[{remote}] 长度字段 {bodyLen} 超过实际数据 {length - 5} 字节，丢弃");
                return;
            }

            // 3. 组装成包对象，按类型分发——body 怎么解析由各处理器自己决定
            var pkt = new GamePacket
            {
                Remote = remote,
                Type = packet[2],
                Buffer = packet,
                Offset = 5,
                Count = bodyLen,
            };

            switch (pkt.Type)
            {
                case PacketTypes.Login: LoginHandler.HandleLogin(pkt); break;
                case PacketTypes.Action1:
                    if (RequireLoggedIn(pkt)) Handle1.HandlePlayerAction1(pkt);
                    break;
                case PacketTypes.Action2:
                    if (RequireLoggedIn(pkt)) Handle2.HandlePlayerAction2(pkt);
                    break;
                default:
                    Console.WriteLine($"[{remote}] 未知类型 {pkt.Type}，丢弃");
                    break;
            }
        }

        // 登录门禁：没登录的客户端不能执行动作，回一句"请先登录"
        private static bool RequireLoggedIn(GamePacket pkt)
        {
            if (ClientManager.IsLoggedIn(pkt.Remote))
                return true;
            Responder.Reply(pkt, PacketTypes.ServerReply, "请先登录");
            return false;
        }
    }
}

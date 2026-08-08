using System.Net.Sockets;
using System.Text;

namespace Protobuflern
{
    // 服务器给单个玩家发回复（定向，不广播）
    internal static class Responder
    {
        // 组帧并发送一句回复：AC AC | type | 长度 | UTF8 文本
        internal static void Reply(GamePacket to, byte type, string message)
        {
            byte[] body = Encoding.UTF8.GetBytes(message);

            byte[] packet = new byte[2 + 1 + 2 + body.Length];
            packet[0] = 0xAC;
            packet[1] = 0xAC;
            packet[2] = type;
            packet[3] = (byte)(body.Length >> 8);
            packet[4] = (byte)body.Length;
            body.CopyTo(packet, 5);

            Program.ServerSocket.SendTo(packet, packet.Length, SocketFlags.None, to.Remote);
        }
    }
}

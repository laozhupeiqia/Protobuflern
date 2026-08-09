using Protobuflern.Demo;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Protobuflern
{
    // 服务器发消息的统一出口：
    // 组帧只有 Frame 一处，接收者怎么选由各方法决定
    internal static class Sender
    {
        // 底层原语：给单个端点发一个包（组帧：AC AC | type | 长度 | body）
        internal static void SendTo(IPEndPoint remote, PacketType type, byte[] body)
        {
            byte[] packet = Frame(type, body);
            Program.ServerSocket.SendTo(packet, packet.Length, SocketFlags.None, remote);
        }

        // 定向回复：给发送者回一句 UTF8 文本
        internal static void Reply(GamePacket to, PacketType type, string message)
            => SendTo(to.Remote, type, Encoding.UTF8.GetBytes(message));

        // 服务器主动广播：给所有在线玩家发一条消息，调用方只传消息本身
        internal static void SendToAll(PacketType type, byte[] body)
        {
            byte[] packet = Frame(type, body);
            SendLoop(packet, packet.Length, null);
        }

        // 转发某个玩家的原始包给除他外的所有玩家（Program.cs 的广播用）
        internal static void SendToAllExcept(IPEndPoint except, byte[] packet, int length)
        {
            SendLoop(packet, length, except);
        }

        // 组帧：AC AC | type | 长度 | body（唯一一处，所有发送共用）
        private static byte[] Frame(PacketType type, byte[] body)
        {
            byte[] packet = new byte[5 + body.Length];
            packet[0] = 0xAC;
            packet[1] = 0xAC;
            packet[2] = (byte)type;
            packet[3] = (byte)(body.Length >> 8);
            packet[4] = (byte)body.Length;
            body.CopyTo(packet, 5);
            return packet;
        }

        // 给一批收件人发一个已组帧的包；except 为空则发给所有人
        // 发失败的记成掉线，遍历完再统一踢——ClientManager.All 是活视图，边遍历边改表会抛异常
        private static void SendLoop(byte[] packet, int length, IPEndPoint? except)
        {
            if (ClientManager.Count == 0) return;
            if (except != null && ClientManager.Count <= 1) return;   // 转发时只有自己，无人可发

            List<IPEndPoint>? dead = null;   // 掉线的（正常时为空，不分配）
            foreach (var remote in ClientManager.All)
            {
                if (remote.Equals(except)) continue;
                try
                {
                    Program.ServerSocket.SendTo(packet, length, SocketFlags.None, remote);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"[发送失败] {remote} 掉线，移除：{ex.Message}");
                    (dead ??= new List<IPEndPoint>()).Add(remote);
                }
            }
            if (dead != null)
                foreach (var ep in dead)
                    ClientManager.Kick(ep);
        }
    }
}

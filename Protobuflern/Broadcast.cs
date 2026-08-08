using System.Net;
using System.Net.Sockets;

namespace Protobuflern
{
    internal class Broadcast
    {
        // 广播原始包给除发送者外的所有客户端；SendTo 失败说明对方已掉线，直接移除
        internal static void ToAll(Socket socket, IPEndPoint from, byte[] packet, int length)
        {
            if (ClientManager.Count <= 1) return;    // 只有自己，无人可广播

            List<IPEndPoint>? dead = null;     // 掉线的（正常时为空，不分配）
            foreach (var remote in ClientManager.All)
            {
                if (remote.Equals(from)) continue;
                try
                {
                    socket.SendTo(packet, length, SocketFlags.None, remote);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"[转发失败] {remote} 掉线，移除：{ex.Message}");
                    (dead ??= new List<IPEndPoint>()).Add(remote);
                }
            }
            if (dead != null)
                foreach (var ep in dead)
                    ClientManager.Kick(ep);
        }
    }
}

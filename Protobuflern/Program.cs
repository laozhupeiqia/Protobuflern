using System.Net;
using System.Net.Sockets;

namespace Protobuflern
{
    internal class Program
    {
        internal static Socket ServerSocket = null!;   // 服务器唯一 Socket（Main 里赋值后才用）
        private static readonly byte[] ReceiveBuffer = new byte[64 * 1024];

        static void Main(string[] args)
        {
            const int port = 9001;
            using var udp = new UdpClient(port);
            ServerSocket = udp.Client;
            Console.WriteLine($"UDP 服务器已启动，监听端口 {port} ...");

            while (true)
            {
                try
                {
                    // 1. 收包（读进池化缓冲区，零分配）
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    int received = udp.Client.ReceiveFrom(ReceiveBuffer, ref remoteEP);
                    IPEndPoint client = (IPEndPoint)remoteEP;

                    // 2. 登记客户端：新加入 / 刷新活跃时间
                    ClientManager.AddOrTouch(client);

                    // 3. 解析并分发
                    FrameParser.Process(client, ReceiveBuffer, received);

                    // 4. 广播给其他客户端
                    Broadcast.ToAll(udp.Client, client, ReceiveBuffer, received);

                    // 5. 到点自动清理超时客户端
                    ClientManager.MaybeCleanup();
                }
                catch (SocketException ex)
                {
                    // 客户端掉线或端口不可达，忽略继续
                    if (ex.SocketErrorCode is SocketError.ConnectionReset or SocketError.ConnectionRefused)
                    {
                        Console.WriteLine($"[掉线] {ex.SocketErrorCode}，忽略");
                        continue;
                    }
                    Console.WriteLine($"[网络异常] {ex.SocketErrorCode} {ex.Message}");
                }
                catch (Exception ex)
                {
                    // 坏包、解析失败等，不能让它们杀死服务器
                    Console.WriteLine($"[处理异常] {ex.Message}");
                }
            }
        }
    }
}

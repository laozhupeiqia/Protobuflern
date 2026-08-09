using System.Net;
using System.Net.Sockets;

namespace ServerFramework.Network;

// 持有 UDP 套接字的传输层：只管"收发字节"，不懂帧、不懂业务
public sealed class NetworkTransport : IDisposable
{
    private readonly UdpClient _udp;

    public NetworkTransport(int port)
    {
        _udp = new UdpClient(port);
    }

    // 阻塞收一个包到缓冲区，返回实际字节数并带出发送端地址
    public int ReceiveInto(byte[] buffer, out IPEndPoint remote)
    {
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        int received = _udp.Client.ReceiveFrom(buffer, ref remoteEP);
        remote = (IPEndPoint)remoteEP;
        return received;
    }

    public void SendTo(IPEndPoint remote, byte[] packet, int length)
        => _udp.Client.SendTo(packet, length, SocketFlags.None, remote);

    public void Dispose() => _udp.Dispose();
}

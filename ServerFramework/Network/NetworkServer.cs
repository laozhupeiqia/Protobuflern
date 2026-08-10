using System.Net.Sockets;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace ServerFramework.Network;

// 服务器主循环：收包 -> 登记/刷新会话 -> 解析帧 -> 分发
// 只做框架层的编排，具体包怎么处理由游戏层 Handler 决定
// 不做定时踢人：会话不会因为沉默被清掉（挂机/网络波动不该掉线），账号复用由登录时判定
public sealed class NetworkServer
{
    private readonly NetworkTransport _transport;
    private readonly SessionManager _sessions;
    private readonly MessageDispatcher _dispatcher;
    private readonly byte[] _receiveBuffer = new byte[64 * 1024];

    public NetworkServer(NetworkTransport transport, SessionManager sessions, MessageDispatcher dispatcher)
    {
        _transport = transport;
        _sessions = sessions;
        _dispatcher = dispatcher;
    }

    public void Run()
    {
        while (true)
        {
            try
            {
                // 1. 收包（读进池化缓冲区，零分配）
                int received = _transport.ReceiveInto(_receiveBuffer, out var remote);

                // 2. 登记会话：新加入 / 刷新活跃时间
                ClientSession session = _sessions.GetOrAdd(remote);

                // 3. 解析并分发（坏帧直接忽略，业务交给 Handler）
                if (FrameParser.Parse(_receiveBuffer, received) is { } msg)
                    _dispatcher.Dispatch(session, msg);
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

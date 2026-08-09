using Protobuflern.Demo;
using Protobuflern.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Protobuflern.Handles
{
    // 类型 -> 处理器 的路由表：FrameParser 按包类型找到对应处理器
    // 只注册"客户端 → 服务器"的请求类型；0x80+ 是服务器回复类型，客户端不会发过来，不进这张表
    // 静态字段初始化时填一次，以后加新协议就往这里加一行
    internal static class HandleManager
    {
        private static readonly Dictionary<PacketType, IHandle> handlers = new()
        {
            [PacketType.Hello] = new HelloHandler(),
            [PacketType.Action1] = new Handle1(),
            [PacketType.Action2] = new Handle2(),
            [PacketType.Login] = new LoginHandler(),
            [PacketType.TestBroadcast] = new BroadcastTestHandler(),
        };

        // 按类型找处理器；没注册返回 false，由调用方决定怎么处理
        // [MaybeNullWhen(false)]：返回 true 时 handler 必非空，调用处不用再判空
        internal static bool TryGetHandler(PacketType type, [MaybeNullWhen(false)] out IHandle handler)
        {
            return handlers.TryGetValue(type, out handler);
        }
    }
}

using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    internal class Handle2 : MessageHandler<Player>
    {
        // 类型 2 的业务：body 反序列化已经由 MessageHandler<Player> 基类完成
        public override void Handle(ClientSession session, Player player)
        {
            if (!session.IsAuthenticated)
            {
                session.Reply((int)PacketType.ServerReply, new MsgResult { Code = 1, Message = "请先登录" });
                return;
            }

            Console.WriteLine($"[{session}] {player.Name} 做了事情2");
            session.Reply((int)PacketType.ServerReply, new MsgResult { Code = 0, Message = "操作完成" });
        }
    }
}

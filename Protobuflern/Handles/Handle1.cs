using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    internal class Handle1 : MessageHandler<Player>
    {
        public override void Handle(ClientSession session, Player player)
        {
            // 登录门禁：没登录不能执行动作，回一句"请先登录"
            if (!session.IsAuthenticated)
            {
                session.Reply((int)PacketType.ServerReply, new MsgResult { Code = 1, Message = "请先登录" });
                return;
            }

            Console.WriteLine($"[{session}] {player.Name} 做了事情1");
            session.Reply((int)PacketType.ServerReply, new MsgResult { Code = 0, Message = "操作完成" });
        }
    }
}

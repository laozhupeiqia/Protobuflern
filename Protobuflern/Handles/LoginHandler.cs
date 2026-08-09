using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 登录业务：收到登录请求 → 标记已登录 → 回"登录成功"
    internal class LoginHandler : MessageHandler<Player>
    {
        public override void Handle(ClientSession session, Player player)
        {
            // 标记已登录并绑定玩家标识：登录成功之前，动作类消息一律被自己的门禁挡下
            session.IsAuthenticated = true;
            session.PlayerId = player.Name;
            Console.WriteLine($"[{session}] {player.Name} 登录成功");

            // 回登录成功（类型 0x81），客户端收到这个才算是"进了游戏"
            session.Reply((int)PacketType.LoginOk, $"登录成功，欢迎 {player.Name}");
        }
    }
}

using Protobuflern.Demo;
using Protobuflern.Database;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 登录业务：校验账号密码 → 成功标记已登录并回玩家状态，失败回错误码
    internal class LoginHandler : MessageHandler<Player>
    {
        public override void Handle(ClientSession session, Player player)
        {
            PlayerState? state = PlayerRepository.Login(int.Parse(player.Id), player.Paswd);

            if (state != null)
            {
                session.IsAuthenticated = true;
                session.PlayerId = state.Name;
                Console.WriteLine($"[{session}] {state.Name} 登录成功");

                session.Reply((int)PacketType.ServerReply, new MsgResult
                {
                    Code = 0,
                    Message = $"登录成功，欢迎 {state.Name}",
                    Player = state
                });
            }
            else
            {
                session.Reply((int)PacketType.ServerReply, new MsgResult
                {
                    Code = 1,
                    Message = "登录失败，账号或密码错误"
                });
            }
        }
    }
}

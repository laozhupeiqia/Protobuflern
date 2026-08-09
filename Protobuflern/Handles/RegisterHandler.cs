using Protobuflern.Demo;
using Protobuflern.Database;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 注册业务：收到注册请求 → 写数据库 → 回结果
    internal class RegisterHandler : MessageHandler<Player>
    {
        public override void Handle(ClientSession session, Player player)
        {
            if (!int.TryParse(player.Id, out int playerId))
            {
                session.Reply((int)PacketType.ServerReply, new MsgResult { Code = 1, Message = "账号格式不对，请输入数字" });
                return;
            }

            bool ok = PlayerRepository.Register(playerId, player.Paswd, player.Name);
            Console.WriteLine($"[{session}] {(ok ? "注册成功" : "注册失败")} 账号 {player.Id}");

            session.Reply((int)PacketType.ServerReply, new MsgResult
            {
                Code = ok ? 0 : 1,
                Message = ok ? "注册成功，请登录" : "注册失败：账号可能已存在"
            });
        }
    }
}

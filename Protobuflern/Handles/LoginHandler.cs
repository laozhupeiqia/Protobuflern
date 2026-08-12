using Protobuflern.Demo;
using Protobuflern.Database;
using ServerFramework.Dispatch;
using ServerFramework.Session;
using System;

namespace Protobuflern.Handles
{
    // 登录业务：校验账号密码 → 成功标记已登录并回玩家状态，失败回错误码
    internal class LoginHandler : MessageHandler<Player>
    {
        // 旧会话超过这么久没发包就视为失联：新登录直接接管（当重连），不报"已在别处登录"
        // 心跳 5 秒一次，留出余量：正常在玩的玩家会话永远"活跃"，别人登不进来；失联的才能被接管
        private const int ReconnectTakeoverSeconds = 20;

        public override void Handle(ClientSession session, Player player)
        {
            PlayerState? state = PlayerRepository.Login(int.Parse(player.Id), player.Paswd);

            if (state != null)
            {
                // 同账号已在线（在别的会话上）：PlayerId 唯一，最多一个，先找出来再处理（避免遍历时改集合）
                ClientSession? other = null;
                foreach (ClientSession s in SessionManager.Instance.All)
                {
                    if (!ReferenceEquals(s, session) && s.PlayerId == player.Id)
                    {
                        other = s;
                        break;
                    }
                }

                if (other != null)
                {
                    // 旧会话还在发包（活跃）= 真·第二台客户端，拒绝本次登录
                    if ((DateTime.UtcNow - other.LastActiveTime).TotalSeconds <= ReconnectTakeoverSeconds)
                    {
                        session.Reply((int)PacketType.ServerReply, new MsgResult
                        {
                            Code = 2,
                            Message = "当前账号已在别处登录"
                        });
                        return;
                    }

                    // 旧会话已失联：当成重连/接管，先清理它所在的房间（房主走了会解散房间并通知剩余成员回单机），再删掉旧会话
                    Console.WriteLine($"[重连] {other} 失联，账号 {player.Id} 由新连接接管");
                    RoomRegistry.RemovePlayerAndNotify(other.PlayerId!);
                    SessionManager.Instance.Kick(other.RemoteEndPoint);
                }

                session.IsAuthenticated = true;
                session.PlayerId = player.Id;   // 绑账号 id（唯一标识），昵称可能重复不能当标识
                Console.WriteLine($"[{session}] {state.Name} 登录成功");

                // 登记在线玩家表（角色数据）
                PlayerRegistry.Register(player.Id, state);

                // 回登录成功：客户端收到后会切到游戏场景
                session.Reply((int)PacketType.ServerReply, new MsgResult
                {
                    Code = 0,
                    Message = $"登录成功，欢迎 {state.Name}",
                    Player = state
                });

                // 在线快照（PLAYER_LIST）不在登录时发——那时客户端正好在切场景，角色会建在登录场景里被吞掉。
                // 改成客户端游戏场景加载完成后、第一次上报位置时再发（见 PlayerFrameHandler）

                // 新玩家上线 = 事件通知：广播刷新所有在线玩家的房间列表（每人排除自己），
                // 旧玩家能立刻看到新玩家上线，新玩家也能看到旧玩家
                RoomRegistry.BroadcastRoomListToAll();
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

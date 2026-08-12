using Google.Protobuf;
using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 实时状态上报：客户端 PLAYER_FRAME 发来 PlayerFrame（位置+朝向+动作），服务器用登录账号做权威 playerId 广播给其他玩家（排除发送者）。
    // 第一个状态包广播"加入"（带角色数据+位置，让在场的人立刻创建），之后广播状态更新。
    internal class PlayerFrameHandler : MessageHandler<PlayerFrame>
    {
        public override void Handle(ClientSession session, PlayerFrame msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            msg.PlayerId = session.PlayerId;

            PlayerState? state = PlayerRegistry.GetState(session.PlayerId);
            if (PlayerRegistry.IsFirstPosition(session.PlayerId) && state != null)
            {
                SessionManager.Instance.Broadcast(session, (int)PacketType.PlayerJoined, new PlayerJoinMsg
                {
                    Player = state,
                    Frame = msg
                }.ToByteArray());

                // 玩家第一次报位置 = 游戏场景已加载完，这时补发在线快照，一进游戏就能看到在场的人
                // （登录时发的快照会落在切场景的过程中被吞掉，改在这发才可靠）
                PlayerListMsg snapshot = PlayerRegistry.BuildSnapshot(session.PlayerId);
                if (snapshot.Players.Count > 0)
                    session.Reply((int)PacketType.PlayerList, snapshot);
            }
            else
            {
                SessionManager.Instance.Broadcast(session, (int)PacketType.SyncPlayerFrame, msg.ToByteArray());
            }

            PlayerRegistry.SetPosition(session.PlayerId, msg);
        }
    }
}

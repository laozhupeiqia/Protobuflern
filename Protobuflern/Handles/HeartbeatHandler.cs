using Google.Protobuf;
using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 心跳：body 是空消息。客户端周期性发来，收到即刷新会话活跃时间（NetworkServer 收包时已做）。
    // 这里额外做一件事：把该玩家最新位置广播给其他人，让静止的玩家（站着不动）远端角色也持续存活。
    // 否则位置包只在移动时发，静止玩家 20 秒就会被别人当掉线移除；心跳 5 秒一次，正好续命。
    internal class HeartbeatHandler : MessageHandler<Heartbeat>
    {
        public override void Handle(ClientSession session, Heartbeat heartbeat)
        {
            if (session.RoomId == null)
                return;   // 单机：不广播

            if (session.IsAuthenticated && session.PlayerId != null &&
                PlayerRegistry.TryGetPosition(session.PlayerId, out PlayerFrame pos))
            {
                RoomRegistry.BroadcastToRoom(session, session, (int)PacketType.SyncPlayerFrame, pos.ToByteArray());
            }
        }
    }
}

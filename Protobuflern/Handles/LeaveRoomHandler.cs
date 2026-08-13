using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;
using System;

namespace Protobuflern.Handles
{
    // 退出联机模式：客户端 LEAVE_ROOM（空 body，用 Heartbeat 当空消息解析）。
    // 普通成员退出 → 从房间移除并通知房主/剩余成员（members=剩余列表，房主保持联机）；
    // 房主退出 → 解散房间并通知所有成员回单机（members 为空）。随后刷新全员房间列表。
    internal class LeaveRoomHandler : MessageHandler<Heartbeat>
    {
        public override void Handle(ClientSession session, Heartbeat msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            if (session.RoomId == null)
                return;   // 单机：不管

            Console.WriteLine($"[退出房间] {session.PlayerId} 离开房间 {session.RoomId}");

            RoomRegistry.RemovePlayerAndNotify(session.PlayerId);
            session.RoomId = null;   // 自己回单机

            RoomRegistry.BroadcastRoomListToAll();
        }
    }
}

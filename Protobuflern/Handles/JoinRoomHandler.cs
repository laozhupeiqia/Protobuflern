using Google.Protobuf;
using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 加入房间：A 发 JOIN_ROOM{hostId=B} → 创建以 B 为房主的房间（不存在才建），成员={B,A}
    // 没有"开房间"消息、没有解散功能（本阶段）；一个玩家最多在一个房间里
    internal class JoinRoomHandler : MessageHandler<JoinRoomMsg>
    {
        public override void Handle(ClientSession session, JoinRoomMsg msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            if (session.RoomId != null)
            {
                session.Reply((int)PacketType.JoinRoomResult, new JoinRoomResult
                {
                    Code = 1,
                    Message = "你已经在多人游戏中了无法退出"
                });
                return;
            }

            if (msg.HostId == session.PlayerId)
            {
                session.Reply((int)PacketType.JoinRoomResult, new JoinRoomResult
                {
                    Code = 2,
                    Message = "不能加入自己的房间"
                });
                return;
            }

            if (PlayerRegistry.GetState(msg.HostId) == null)
            {
                session.Reply((int)PacketType.JoinRoomResult, new JoinRoomResult
                {
                    Code = 3,
                    Message = "房主不在线"
                });
                return;
            }

            // 成功：懒创建房间并加入
            RoomRegistry.Room room = RoomRegistry.GetOrCreate(msg.HostId);
            RoomRegistry.AddMember(msg.HostId, session.PlayerId);
            session.RoomId = msg.HostId;

            // 房主也进入联机：找到房主会话，若其在别的房间则从旧房间移除并改成自己房间
            foreach (ClientSession s in SessionManager.Instance.All)
            {
                if (s.PlayerId != msg.HostId) continue;
                if (s.RoomId == null)
                {
                    s.RoomId = msg.HostId;
                }
                else if (s.RoomId != msg.HostId)
                {
                    RoomRegistry.RemovePlayer(s.PlayerId!);
                    s.RoomId = msg.HostId;
                }
                break;
            }

            // 回加入者：成功结果带完整成员列表
            var result = new JoinRoomResult
            {
                Code = 0,
                Message = "加入成功",
                HostId = msg.HostId
            };
            foreach (string member in room.Members) result.Members.Add(member);
            session.Reply((int)PacketType.JoinRoomResult, result);

            // 给房间内"除加入者外"的成员广播成员变动
            var changed = new RoomMemberChangedMsg
            {
                HostId = msg.HostId,
                MemberId = session.PlayerId,
                Joined = true
            };
            foreach (string member in room.Members) changed.Members.Add(member);
            RoomRegistry.BroadcastToRoomMembers(msg.HostId, session, (int)PacketType.RoomMemberChanged, changed.ToByteArray());

            // 列表人数刷新
            RoomRegistry.BroadcastRoomListToAll();
        }
    }
}

using Protobuflern.Demo;
using ServerFramework.Session;

namespace Protobuflern
{
    // 房间注册表：房主账号 → 房间（Members 始终含房主）
    // 房间懒创建：没有"开房间"消息，被加入时才建；本阶段没有解散 UI，掉线/被踢由 RemovePlayerAndNotify 清理
    internal static class RoomRegistry
    {
        // 房间：房主账号即房间唯一标识，Members 始终含房主
        internal sealed class Room
        {
            public readonly string HostId;
            public readonly HashSet<string> Members = new();

            public Room(string hostId)
            {
                HostId = hostId;
                Members.Add(hostId);
            }
        }

        private static readonly Dictionary<string, Room> _rooms = new();

        // 懒创建：不存在才建（建时把 host 加进 Members）
        public static Room GetOrCreate(string hostId)
        {
            if (!_rooms.TryGetValue(hostId, out Room? room))
            {
                room = new Room(hostId);
                _rooms[hostId] = room;
            }
            return room;
        }

        public static Room? Get(string hostId)
            => _rooms.TryGetValue(hostId, out Room? room) ? room : null;

        public static void AddMember(string hostId, string account)
            => GetOrCreate(hostId).Members.Add(account);

        // 找 account 所在的房间；不在任何房间返回 null
        public static Room? FindRoomOf(string account)
        {
            foreach (Room room in _rooms.Values)
            {
                if (room.Members.Contains(account)) return room;
            }
            return null;
        }

        // 把 account 从它所在的房间移除；如果它是房主则解散房间并返回 true，否则返回 false
        public static bool RemovePlayer(string account)
        {
            foreach (KeyValuePair<string, Room> kv in _rooms)
            {
                if (kv.Value.Members.Remove(account))
                {
                    if (kv.Key == account)
                    {
                        _rooms.Remove(kv.Key);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        // 有房间返回成员数，没有返回 1（孤零零的在线玩家也是 1 人的房间）
        public static int CountOf(string hostId)
            => _rooms.TryGetValue(hostId, out Room? room) ? room.Members.Count : 1;

        // 房间列表 = 所有在线玩家，每人一条 RoomInfo（HostName 取角色昵称），跳过 self
        public static RoomListMsg BuildRoomList(string selfAccount)
        {
            var list = new RoomListMsg();
            foreach (KeyValuePair<string, PlayerState> kv in PlayerRegistry.AllStates)
            {
                if (kv.Key == selfAccount) continue;
                list.Rooms.Add(new RoomInfo
                {
                    HostId = kv.Key,
                    HostName = kv.Value.Name,
                    Count = CountOf(kv.Key)
                });
            }
            return list;
        }

        // 按发送者所在房间广播，排除 exclude；member.RoomId 为 null（单机）直接 return 不广播
        public static void BroadcastToRoom(ClientSession member, ClientSession? exclude, int messageId, byte[] body)
        {
            if (member.RoomId == null) return;
            BroadcastToRoomMembers(member.RoomId, exclude, messageId, body);
        }

        // 按 hostId 拿房间，给房间内除 exclude 外的成员会话发送；房间不存在则不广播
        public static void BroadcastToRoomMembers(string hostId, ClientSession? exclude, int messageId, byte[] body)
        {
            Room? room = Get(hostId);
            if (room == null) return;
            foreach (ClientSession session in SessionManager.Instance.All)
            {
                if (exclude != null && session == exclude) continue;
                if (session.PlayerId != null && room.Members.Contains(session.PlayerId))
                    session.Send(messageId, body);
            }
        }

        // 给所有在线会话每人发一份"排除自己"的 ROOM_LIST（列表刷新用）
        public static void BroadcastRoomListToAll()
        {
            foreach (ClientSession session in SessionManager.Instance.All)
            {
                if (session.PlayerId == null) continue;
                session.Reply((int)PacketType.RoomList, BuildRoomList(session.PlayerId));
            }
        }

        // 掉线/被踢清理：把 account 从房间移除；房主走了则解散房间并通知剩余成员回单机
        public static void RemovePlayerAndNotify(string account)
        {
            Room? room = FindRoomOf(account);
            if (room == null) return;

            bool dissolved = RemovePlayer(account);
            if (!dissolved) return;   // 普通成员离开，其他人不受影响

            // 房主走了：房间已解散，通知剩余成员回单机
            if (room.Members.Count == 0) return;

            // Members 传空：客户端收到 joined=false 且空列表 → 判定房间解散、ResetRoom 回单机
            var changed = new RoomMemberChangedMsg
            {
                HostId = room.HostId,
                MemberId = account,
                Joined = false
            };

            foreach (ClientSession session in SessionManager.Instance.All)
            {
                if (session.PlayerId != null && room.Members.Contains(session.PlayerId))
                {
                    session.RoomId = null;   // 回到单机
                    session.Reply((int)PacketType.RoomMemberChanged, changed);
                }
            }
        }
    }
}

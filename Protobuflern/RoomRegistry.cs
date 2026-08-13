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

        // 房间列表 = 所有在线玩家，每人一条 RoomInfo（含自己：客户端按 isSelf 显示人数但隐藏加入按钮，
        // 让房主也能看到自己房间的人数变化）
        public static RoomListMsg BuildRoomList()
        {
            var list = new RoomListMsg();
            foreach (KeyValuePair<string, PlayerState> kv in PlayerRegistry.AllStates)
            {
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
                session.Reply((int)PacketType.RoomList, BuildRoomList());
            }
        }

        // 离开/掉线清理：把 account 从房间移除。普通成员离开 → 通知剩余成员（含房主）members=剩余列表，房主保持联机；
        // 房主离开 → 房间解散，通知剩余成员 members 为空（客户端判定解散 ResetRoom 回单机）
        public static void RemovePlayerAndNotify(string account)
        {
            Room? room = FindRoomOf(account);
            if (room == null) return;

            bool dissolved = RemovePlayer(account);

            var changed = new RoomMemberChangedMsg
            {
                HostId = room.HostId,
                MemberId = account,
                Joined = false
            };

            // 普通成员离开：members 带剩余成员；房主解散：members 保持空 → 客户端据空列表判定解散
            if (!dissolved)
            {
                foreach (string m in room.Members) changed.Members.Add(m);
            }

            foreach (ClientSession session in SessionManager.Instance.All)
            {
                if (session.PlayerId != null && room.Members.Contains(session.PlayerId))
                {
                    if (dissolved) session.RoomId = null;   // 只有解散才把剩余成员会话重置回单机
                    session.Reply((int)PacketType.RoomMemberChanged, changed);
                }
            }
        }
    }
}

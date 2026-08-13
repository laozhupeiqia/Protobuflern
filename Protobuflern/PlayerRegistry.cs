using Protobuflern.Demo;

namespace Protobuflern
{
    // 在线玩家表：账号 → 角色数据 + 最后已知位置
    // 登录时登记角色数据，位置包到达时记录位置
    // 服务器靠它：登录时给新人发在线快照（PLAYER_LIST）、新人第一个位置包触发"加入"广播（PLAYER_JOINED）
    internal static class PlayerRegistry
    {
        private static readonly Dictionary<string, PlayerState> _states = new();
        private static readonly Dictionary<string, PlayerFrame> _positions = new();

        // 登录成功时登记角色数据（账号已绑定的会话、重连接管时都会走到）
        public static void Register(string account, PlayerState state)
        {
            _states[account] = state;
        }

        // 掉线清理：账号从在线表移除（超时会话/接管/房间解散时调用，避免残留幽灵玩家）
        public static void Remove(string account)
        {
            _states.Remove(account);
            _positions.Remove(account);
            // 在线玩家表变了 → 房间列表（从玩家表派生）必须给所有在线玩家刷新，
            // 否则别人客户端还留着已掉线玩家的"房间按钮"（超时清理这条路径最容易漏）
            RoomRegistry.BroadcastRoomListToAll();
        }

        public static PlayerState? GetState(string account)
            => _states.TryGetValue(account, out var s) ? s : null;

        // 该账号是否还没发过位置（第一次位置包触发"加入"广播）
        public static bool IsFirstPosition(string account) => !_positions.ContainsKey(account);

        public static void SetPosition(string account, PlayerFrame pos)
        {
            _positions[account] = pos;
        }

        public static bool TryGetPosition(string account, out PlayerFrame pos)
            => _positions.TryGetValue(account, out pos!);

        // 遍历全部在线角色数据（组快照用）
        public static IEnumerable<KeyValuePair<string, PlayerState>> AllStates => _states;

        // 组在线快照：发给新加入的玩家，让他立刻看到已经在场的人（不含自己、不含还没报过位置的）
        // 在"玩家第一次上报位置"时发送——那时客户端游戏场景已加载，快照里的角色会建在正确的场景里
        public static PlayerListMsg BuildSnapshot(string selfAccount)
        {
            var list = new PlayerListMsg();
            foreach (KeyValuePair<string, PlayerState> kv in _states)
            {
                if (kv.Key == selfAccount) continue;
                if (!TryGetPosition(kv.Key, out PlayerFrame pos)) continue;
                list.Players.Add(new PlayerJoinMsg { Player = kv.Value, Frame = pos });
            }
            return list;
        }
    }
}

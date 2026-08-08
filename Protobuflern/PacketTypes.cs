namespace Protobuflern
{
    // 包类型统一收在这里，避免魔法数字散落各处
    internal static class PacketTypes
    {
        public const byte Action1 = 0x01;       // 玩家动作1（客户端输入 1）
        public const byte Action2 = 0x02;       // 玩家动作2（客户端输入 2）
        public const byte ServerReply = 0x10;   // 服务器普通回复（文本）
        public const byte Login = 0x11;         // 登录请求（客户端 → 服务器，body 是 Player）
        public const byte LoginOk = 0x12;       // 登录成功（服务器 → 客户端，body 是文本）
    }
}

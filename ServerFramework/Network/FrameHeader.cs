namespace ServerFramework.Network;

// 帧头定义：AC AC | MessageId(1字节) | BodyLength(2字节大端) | Body
// 帧格式刻意保持简单、兼容旧客户端；Version/Flags/Sequence 只是为将来
// 可靠 UDP / 协议升级预留的注释，暂时不占线上字节。
public readonly struct FrameHeader
{
    public const byte MagicHigh = 0xAC;
    public const byte MagicLow = 0xAC;
    public const int HeaderSize = 5;   // 2 魔数 + 1 MessageId + 2 长度

    public byte MessageId { get; }
    public int BodyLength { get; }

    private FrameHeader(byte messageId, int bodyLength)
    {
        MessageId = messageId;
        BodyLength = bodyLength;
    }

    // 校验魔数 + 数据长度，全部通过才算一个合法帧
    public static bool TryParse(byte[] buffer, int length, out FrameHeader header)
    {
        header = default;
        if (length < HeaderSize || buffer[0] != MagicHigh || buffer[1] != MagicLow)
            return false;
        int bodyLength = (buffer[3] << 8) | buffer[4];
        if (length < HeaderSize + bodyLength)
            return false;
        header = new FrameHeader(buffer[2], bodyLength);
        return true;
    }
}

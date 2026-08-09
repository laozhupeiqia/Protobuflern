namespace ServerFramework.Network;

// 纯网络协议解析器：校验帧头 -> 取出 MessageId -> 切出 Body -> 返回 NetworkMessage
// 不含任何业务判断（登录、动作、广播等都是游戏层的事）
public static class FrameParser
{
    public static NetworkMessage? Parse(byte[] packet, int length)
    {
        if (!FrameHeader.TryParse(packet, length, out FrameHeader header))
            return null;

        byte[] body = new byte[header.BodyLength];
        Array.Copy(packet, FrameHeader.HeaderSize, body, 0, header.BodyLength);
        return new NetworkMessage(header.MessageId, body);
    }
}

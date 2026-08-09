namespace ServerFramework.Network;

// 组帧：所有发送共用这一处（AC AC | MessageId | BodyLength | Body）
public static class FrameCodec
{
    public static byte[] Encode(int messageId, byte[] body)
    {
        byte[] packet = new byte[FrameHeader.HeaderSize + body.Length];
        packet[0] = FrameHeader.MagicHigh;
        packet[1] = FrameHeader.MagicLow;
        packet[2] = (byte)messageId;
        packet[3] = (byte)(body.Length >> 8);
        packet[4] = (byte)body.Length;
        body.CopyTo(packet, FrameHeader.HeaderSize);
        return packet;
    }
}

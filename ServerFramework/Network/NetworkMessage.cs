namespace ServerFramework.Network;

// 一帧解析出来的结果：MessageId + Body
// Body 是独立副本，Handler 可以安全持有，不受收包缓冲区复用影响
public readonly struct NetworkMessage
{
    public int MessageId { get; }
    public byte[] Body { get; }

    public NetworkMessage(int messageId, byte[] body)
    {
        MessageId = messageId;
        Body = body;
    }
}

using Protobuflern.Demo;
using System.Net;

// 一个收到的数据包：谁发的 + 类型 + body 在缓冲区里的切片
internal struct GamePacket
{
    public IPEndPoint Remote;   // 发送者
    public PacketType Type;     // 包类型（枚举，来自 proto 生成）
    public byte[] Buffer;       // 完整包所在的缓冲区（池化复用）
    public int Offset;          // body 起点
    public int Count;           // body 长度
}

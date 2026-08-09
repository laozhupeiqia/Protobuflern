using Protobuflern;

namespace Protobuflern.Interfaces
{
    internal interface IHandle
    {
       void Handle(GamePacket pkt);
    }
}

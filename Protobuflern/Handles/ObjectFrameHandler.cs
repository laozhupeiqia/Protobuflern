using Google.Protobuf;
using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 物体状态上报：客户端 OBJECT_FRAME(0x0E) 发来 ObjectFrame（房主世界权威），服务器广播给本房间其他玩家（排除发送者）。
    // 单机（RoomId==null）不做任何广播。
    internal class ObjectFrameHandler : MessageHandler<ObjectFrame>
    {
        public override void Handle(ClientSession session, ObjectFrame msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            if (session.RoomId == null)
                return;   // 单机：不广播

            RoomRegistry.BroadcastToRoom(session, session, (int)PacketType.SyncObjectFrame, msg.ToByteArray());
        }
    }
}

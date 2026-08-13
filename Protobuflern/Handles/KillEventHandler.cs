using Google.Protobuf;
using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 击杀事件上报：客户端 KILL_EVENT 发来 KillEvent，服务器用登录账号做权威 killerId，广播给其他玩家（排除发送者）
    internal class KillEventHandler : MessageHandler<KillEvent>
    {
        public override void Handle(ClientSession session, KillEvent msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            if (session.RoomId == null)
                return;   // 单机：不广播

            msg.KillerId = session.PlayerId;
            RoomRegistry.BroadcastToRoom(session, session, (int)PacketType.SyncKillEvent, msg.ToByteArray());
        }
    }
}

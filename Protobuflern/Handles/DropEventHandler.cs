using Google.Protobuf;
using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 获得装备事件上报：客户端 DROP_EVENT 发来 DropEvent，服务器用登录账号做权威 playerId，广播给其他玩家（排除发送者）
    internal class DropEventHandler : MessageHandler<DropEvent>
    {
        public override void Handle(ClientSession session, DropEvent msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            msg.PlayerId = session.PlayerId;
            SessionManager.Instance.Broadcast(session, (int)PacketType.SyncDropEvent, msg.ToByteArray());
        }
    }
}

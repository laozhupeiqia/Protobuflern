using Google.Protobuf;
using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 操作上报：客户端 ACTION 发来 ActionMsg，服务器用登录账号做权威 playerId，广播给其他玩家（排除发送者）
    internal class ActionHandler : MessageHandler<ActionMsg>
    {
        public override void Handle(ClientSession session, ActionMsg msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            msg.PlayerId = session.PlayerId;
            SessionManager.Instance.Broadcast(session, (int)PacketType.SyncAction, msg.ToByteArray());
        }
    }
}

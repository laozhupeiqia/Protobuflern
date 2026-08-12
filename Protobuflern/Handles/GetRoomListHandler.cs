using Protobuflern.Demo;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 请求房间列表：客户端 GET_ROOM_LIST 空 body，用 Heartbeat 当空消息解析（Heartbeat.Parser 解析空 body 没问题）
    internal class GetRoomListHandler : MessageHandler<Heartbeat>
    {
        public override void Handle(ClientSession session, Heartbeat msg)
        {
            if (!session.IsAuthenticated || session.PlayerId == null)
                return;

            session.Reply((int)PacketType.RoomList, RoomRegistry.BuildRoomList(session.PlayerId));
        }
    }
}

using Protobuflern.Demo;
using Protobuflern.Sends;
using ServerFramework.Dispatch;
using ServerFramework.Session;

namespace Protobuflern.Handles
{
    // 测试用：客户端发 0x04，服务器就给全服广播一封"你好世界"邮件
    // 想验证全服广播，开两个客户端，其中一个发 0x04，两个都能收到邮件
    internal class BroadcastTestHandler : MessageHandler<Player>
    {
        public override void Handle(ClientSession session, Player message)
        {
            Console.WriteLine($"[{session}] 请求全服广播");
            SendEmail.SendTextEmail("你好世界");
        }
    }
}

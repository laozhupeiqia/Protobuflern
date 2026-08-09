using Protobuflern.Demo;
using System.Text;

namespace Protobuflern.Sends
{
    // 服务器主动给玩家发邮件：把文本广播给全服所有在线玩家
    internal class SendEmail
    {
        public static void SendTextEmail(string mesg)
        {
            Sender.SendToAll(PacketType.Mail, Encoding.UTF8.GetBytes(mesg));
        }
    }
}

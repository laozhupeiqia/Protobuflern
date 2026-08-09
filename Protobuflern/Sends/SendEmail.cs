using System.Text;
using Protobuflern.Demo;
using ServerFramework.Session;

namespace Protobuflern.Sends
{
    // 服务器主动给玩家发邮件：把文本广播给全服所有在线玩家
    internal static class SendEmail
    {
        public static void SendTextEmail(string mesg)
        {
            SessionManager.Instance.Broadcast((int)PacketType.Mail, Encoding.UTF8.GetBytes(mesg));
        }
    }
}

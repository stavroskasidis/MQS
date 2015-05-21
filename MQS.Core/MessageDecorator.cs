using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace MQS.Core
{
    internal static class MessageDecorator
    {
        private static Regex messageRegex = new Regex(Constants.MessagePattern,RegexOptions.Singleline);

        public static string PrepareMessage(IMessage message, string recipientIP, int recipientPort)
        {
            string typeName = message.GetType().Name;
            return String.Format(Constants.MessageFormat, recipientIP, recipientPort, typeName, message.Serialize());
        }

        public static string GetMessageTypeName(string decoratedMessage)
        {
            return messageRegex.Match(decoratedMessage).Groups[3].Value;
        }

        public static IPEndPoint GetRecipientEndPoint(string decoratedMessage)
        {
            Match match = messageRegex.Match(decoratedMessage);
            string ipAddress = match.Groups[1].Value;
            string port = match.Groups[2].Value;
            return Utilities.GetIPEndPointFromHostName(ipAddress, int.Parse(port), false);//new IPEndPoint(IPAddress.Parse(ipAddress),int.Parse(port));
        }

        public static string UndecorateMessage(string decoratedMessage)
        {
            return messageRegex.Match(decoratedMessage).Groups[4].Value;
        }
    }
}

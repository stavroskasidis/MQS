using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQS.Core
{
    //delegate void MessageReadHandler(MessageReadEventArgs args);

    public interface IMessageListener<out T> where T : IMessage
    {
        void OnMessageRead(MessageReadEventArgs args); 
    }
}

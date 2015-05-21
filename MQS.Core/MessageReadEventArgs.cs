using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQS.Core
{
    //interface IMessageReadEventArgs
    //{
    //    IMessage Message { get; set; }
    //}

    public class MessageReadEventArgs //where T : IMessage
    {
        public IMessage Message { get; set; }
    }
}

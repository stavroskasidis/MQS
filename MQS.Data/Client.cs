using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQS.Data
{

    public class Client : BasicObject
    {
        [Index("IPEndPoint_Index")]
        public string IPEndPoint { get; set; }

        public virtual List<PendingMessage> PendingMessages { get; set; }
    }
}

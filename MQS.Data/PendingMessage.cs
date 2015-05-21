using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace MQS.Data
{
    public class PendingMessage : BasicObject
    {
        public string SerializedMessage { get; set; }

        public string Client_ID { get; set; }

        [Index("Client_Index")]
        [ForeignKey("Client_ID")]
        public Client Client { get; set; }
    }
}

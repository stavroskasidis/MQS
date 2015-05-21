using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace MQS.Data
{
    public abstract class BasicObject
    {
        [Key]
        public virtual string ID { get; set; }

        //[Index("CreatedAt_Index")]
        public DateTime CreatedAt { get; set; }

        public BasicObject()
        {
            CreatedAt = DateTime.Now;
            ID = Guid.NewGuid().ToString();
        }
    }
}

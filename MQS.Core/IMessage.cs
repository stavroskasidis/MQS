using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;

namespace MQS.Core
{
    public interface IMessage
    {
        string Serialize();

        void Deserialize(string serializedMessage);
    }
}

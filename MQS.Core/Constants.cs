using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQS.Core
{
    public static class Constants
    {
        public const char EndOfMessageChar = '\x1B';
        public const string MessageFormat = "IP:{0}:PORT:{1}:TYPE:{2}:DATA:{3}\x1B";
        public const string MessagePattern = "IP:([^:]*):PORT:([^:]*):TYPE:([^:]*):DATA:([^\x1B]*)\x1B";
    }
}

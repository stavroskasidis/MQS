using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQS.Data
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(string name, bool unique = false)
        {
            this.Name = name;
            this.IsUnique = unique;
        }

        public string Name { get; private set; }

        public bool IsUnique { get; private set; }
    }
}

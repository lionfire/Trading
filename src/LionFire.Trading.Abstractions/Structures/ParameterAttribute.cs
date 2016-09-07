using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ParameterAttribute : Attribute
    {

        public ParameterAttribute(string description)
        {
            this.Description = description;
        }

        public string Description {
            get; private set;
        }

        public object DefaultValue { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public object Step { get; set; }
    }
}

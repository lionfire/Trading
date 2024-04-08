using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Parsing.String
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class UnitAttribute : Attribute
    {
        public string Unit { get; private set; } = null!;

        public bool IsPrefix { get; set; }

        public UnitAttribute(string unit)
        {
            _ctor(unit, null);
        }
        public UnitAttribute(string unit, bool isPrefix)
        {
            _ctor(unit, isPrefix);
        }

        private void _ctor(string unit, bool? isPrefix = null)
        {
            this.Unit = unit;
            if (isPrefix.HasValue)
            {
                this.IsPrefix = isPrefix.Value;
            }
            else
            {
                if (unit.EndsWith("="))
                {
                    IsPrefix = true;
                }
                else
                {
                    IsPrefix = false;
                }
            }
        }

    }
}

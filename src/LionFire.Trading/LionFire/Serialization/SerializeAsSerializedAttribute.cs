using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Serialization;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field)]
public class SerializeAsSerializedAttribute : Attribute
{

    public bool SerializeAsSerialized { get; set; }
    public SerializeAsSerializedAttribute() { SerializeAsSerialized = true; }
    public SerializeAsSerializedAttribute(bool serializeAsSerialized) { SerializeAsSerialized = serializeAsSerialized; }
}

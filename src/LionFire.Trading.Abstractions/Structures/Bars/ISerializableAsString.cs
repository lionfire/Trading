using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Serialization;

public interface ISerializableAsString
{
    string? Serialize(); // RENAME: AsString
    abstract static object? Deserialize(string? serializedString); // RENAME: FromString
}

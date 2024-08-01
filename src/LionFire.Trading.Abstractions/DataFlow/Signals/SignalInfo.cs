using System.Reflection;

namespace LionFire.Trading.DataFlow; // TODO: Move to .DataFlow namespace

/// <summary>
/// Contains Binding info to a Slot, on an Instance (not parameters) (doesn't contain Source info)
/// </summary>
/// <param name="PropertyInfo"></param>
public readonly record struct SignalInfo(PropertyInfo PropertyInfo);



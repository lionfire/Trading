using System.Reflection;

namespace LionFire.Trading.Automation.Bots;


/// <summary>
/// 
/// </summary>
/// <param name="Parameter">Property on IPBot2</param>
/// <param name="Values">Corresponding property on IBot2</param>
public readonly record struct InputParameterToValueMapping(PropertyInfo Parameter, PropertyInfo Values);

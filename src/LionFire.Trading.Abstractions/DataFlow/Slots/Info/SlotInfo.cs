using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.DataFlow;

public enum SlotKind
{
    Unspecified = 0,
    ProcessorValuesWindow = 1 << 0,
    InputComponent = 1 << 1,
}

public class SlotInfo
{
    public Type? ValueType
    {
        get
        {
            if (valueType == null && ParameterProperty != null)
            {
                var type = ParameterProperty.PropertyType;
                valueType = ReferenceToX.GetValueType(type);
            }
            return valueType;
        }
        set => valueType = value;
    }
    private Type? valueType;

    #region Parameters Properties

    public PropertyInfo? ParameterProperty { get; set; }
    public PropertyInfo? SourceProperty { get; set; }

    #endregion

    #region Processor Properties

    public PropertyInfo? ProcessorValuesWindowProperty { get; set; }

    #endregion

    public SlotKind Kind => ProcessorValuesWindowProperty != null ? SlotKind.ProcessorValuesWindow : SlotKind.InputComponent;

}

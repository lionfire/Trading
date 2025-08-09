#if TODO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators;

public class CVD
    : MultiInputIndicatorBase<CVD, uint, double, double, double>
    , IIndicator<CVD, uint, (double, double), double>
{
    public override uint Lookback => throw new NotImplementedException();

    public static IOComponent Characteristics(uint parameter)
    {
        throw new NotImplementedException();
    }

    public static CVD Create(uint p)
    {
        throw new NotImplementedException();
    }

    public override void OnNext(IReadOnlyList<(double, double)> value)
    {
        throw new NotImplementedException();
    }
}

#endif
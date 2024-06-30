using LionFire.Trading.Automation.Bots;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Automation;

public interface IPSymbolBarsBot2
{
    ExchangeSymbol ExchangeSymbol { get; }
}
public abstract class PSymbolBarsBot2<TConcrete> : PTimeFrameBot2<TConcrete>,
    IPSymbolBarsBot2
{
    public PSymbolBarsBot2(ExchangeSymbol e, TimeFrame timeFrame) : base(timeFrame)
    {
        ExchangeSymbol = e;
    }

    public PSymbolBarsBot2(ExchangeSymbolTimeFrame e) : base(e.TimeFrame)
    {
        ExchangeSymbol = e;
    }

    public ExchangeSymbol ExchangeSymbol { get; init; }


    public void InitFromDefault()
    {
        var pBotInfo = PBotInfos.Get(this.GetType());

        if (pBotInfo.Bars != null)
        {
            if (pBotInfo.Bars.PropertyType == typeof(SymbolValueAspect<double>))
            {
                pBotInfo.Bars.SetValue(this, new SymbolValueAspect<double>(ExchangeSymbol.Exchange, ExchangeSymbol.ExchangeArea, ExchangeSymbol.Symbol, TimeFrame, DataPointAspect.Close));
            //Bars = new SymbolValueAspect<double>("Binance", "futures", symbol, TimeFrame.m1, DataPointAspect.Close),
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

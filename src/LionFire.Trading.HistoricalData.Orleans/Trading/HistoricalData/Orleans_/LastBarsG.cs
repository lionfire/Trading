using LionFire.Trading.HistoricalData.Orleans_;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Logging;
using Orleans.Utilities;

namespace LionFire.Trading.HistoricalData;

public interface ILastBarsG : IGrainWithStringKey
{

    Task<Guid> Subscribe(ILastBarsO lastBarsO, int bars);
}

public interface ILastBarsO : IGrainObserver
{
    Task OnBars([Immutable] IEnumerable<IKline> kline);
    Task OnBar([Immutable] IKline kline);
}

public class LastBarsO : ILastBarsO
{
    public LastBarsO(ILogger<LastBarsO> logger)
    {
        Logger = logger;
    }

    public ILogger<LastBarsO> Logger { get; }

    public Task OnBar(IKline kline)
    {
        Logger.LogInformation(kline.ToString());
        return Task.CompletedTask;
    }

    public Task OnBars([Immutable] IEnumerable<IKline> kline)
    {
        throw new NotImplementedException();
    }
}
public class LastBarsG : Grain, IGrainWithStringKey
{
    public string? Exchange { get; }
    public string? ExchangeArea { get; }
    public string? Symbol { get; }
    public TimeFrame TimeFrame { get; init; }

    public Exception InvalidFormat() => new ArgumentException("Invalid key format.  Expected: <exchange>.<area>:<symbol> <tf>");
    public LastBarsG(ISymbolIdParser symbolIdParser, OrleansBars orleansBars, ILogger<LastBarsG> logger)
    {
        OrleansBars = orleansBars;
        Logger = logger;
        var s = this.GetPrimaryKeyString().Split(' ');
        if (s.Length != 2) throw InvalidFormat();

        var result = symbolIdParser.TryParse(s[0]);
        if (result == null) throw InvalidFormat();

        this.Exchange = result.ExchangeCode;
        this.ExchangeArea = result.ExchangeAreaCode;
        this.Symbol = result.SymbolCode;

        TimeFrame = TimeFrame.Parse(s[1]);
        observerManager = new(ExpirationDuration, logger);
    }

    #region State

    public class ObserverSettings
    {
        public int Bars { get; set; }
        public DateTime NextOpenTimeToSend { get; set; } = DateTime.MinValue;
    }

    public TimeSpan ExpirationDuration { get; set; } = TimeSpan.FromMinutes(5);
    public Task<TimeSpan> Expiration() => Task.FromResult(ExpirationDuration);
    private ObserverManager<Guid, ILastBarsO> observerManager;
    private Dictionary<Guid, ObserverSettings> observerSettings = new();
    private void ClearExpired()
    {
        observerManager.ClearExpired();
        foreach (var guid in observerSettings.Keys.ToArray())
        {
            if (!observerManager.Observers.ContainsKey(guid))
            {
                observerSettings.Remove(guid);
            }
        }
    }

    public int BarsDesired
    {
        get
        {
            ClearExpired();
            int bars = 0;
            foreach (var observer in observerManager.Observers)
            {
                bars = Math.Max(bars, observerSettings[observer.Key].Bars);
            }
            return bars;
        }
    }

    public OrleansBars OrleansBars { get; }
    public ILogger<LastBarsG> Logger { get; }


    #endregion

    #region Event Handling

    public Task OnBar(IKline kline)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Methods: Grain implementation

    public async Task<Guid> Subscribe(ILastBarsO lastBarsO, int bars)
    {
        var guid = Guid.NewGuid();

        var nextBarOpen = TimeFrame.NextBarOpen(DateTime.UtcNow).DateTime;
        var firstOpenDesired = nextBarOpen - (TimeFrame.TimeSpan!.Value * bars);
        var s = new ObserverSettings
        {
            Bars = bars,
            NextOpenTimeToSend = firstOpenDesired,
        };

        await CatchUp(lastBarsO, s);

        this.observerManager.Subscribe(guid, lastBarsO);

        return guid;
    }

    private async Task CatchUp(ILastBarsO o, ObserverSettings s)
    {
        var nextBarOpen = TimeFrame.NextBarOpen(DateTime.UtcNow).DateTime;

        var bars = await OrleansBars.Bars(new SymbolBarsRange(Exchange, ExchangeArea, Symbol, TimeFrame, s.NextOpenTimeToSend, nextBarOpen));

        await o.OnBars(bars);
        s.NextOpenTimeToSend = nextBarOpen;
    }

    #endregion
}

using Microsoft.Extensions.DependencyInjection;
using Orleans;

namespace LionFire.Trading.HistoricalData.Orleans_;

public class HistoricalDataChunkGrain : Grain, IGrainWithStringKey
{
    public HistoricalDataChunkGrainKey? DataKey { get; private set; }
    public object? Result { get; private set; }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        DataKey = HistoricalDataChunkGrainKey.Parse(this.GetPrimaryKeyString());

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<T[]?> Get<T>(QueryOptions? options = null)
    {
        var k = DataKey;

        if (k.Type != typeof(T)) { throw new ArgumentException($"Generic Type parameter ({typeof(T).FullName}) must match Grain key ({k.Type.FullName})"); }

        options = QueryOptions.Default;
        
        if (Result != null && options.RetrieveSources.HasFlag(HistoricalDataSourceKind.InMemory)) { return (T[]?)Result; }

        var hdp = ServiceProvider.GetRequiredService<IHistoricalDataProvider2>();

        var result = await hdp.Get<T>(k.TimeFrame, k.Symbol, k.Start, k.EndExclusive, new HistoricalDataQueryParameters
        {
            Exchange = k.Exchange,
            ExchangeArea = k.ExchangeArea,
            Options = options
        }).ConfigureAwait(false);

        Result = result;

        return result;
    }
}



#nullable enable

using LionFire;
using LionFire.Trading.Algos.Modular.Bias;
using LionFire.Trading.Algos.Modular.Filters;
using LionFire.Trading.Algos.Parameters;

namespace LionFire.Trading.Algos.Modular
{
    public class ModularAlgoParameters : ITradesMultipleSymbols, IWatchesSymbols
    {
        public List<SymbolIdentifier>? Symbols { get; set; }
        public List<SymbolIdentifier>? SymbolsToWatch { get; set; }

        public List<IEntrySignalProvider>? EntrySignals { get; set; }
        public List<IExitSignalProvider>? ExitSignals { get; set; }
        public List<IBiasProvider>? BiasProviders { get; set; }
        public List<IFilterProvider>? EntryFilterProviders { get; set; }
        public List<IFilterProvider>? ExitFilterProviders { get; set; }
    }

    public interface IEntrySignalProvider { }
    public interface IExitSignalProvider { }
}

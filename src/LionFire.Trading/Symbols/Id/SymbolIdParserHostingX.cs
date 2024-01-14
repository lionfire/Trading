using LionFire.Trading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.Symbols;

namespace LionFire.Hosting;

public static class SymbolIdParserHostingX
{
    public static IServiceCollection AddSymbolIdParser(this IServiceCollection services)
    {
        return services
            .AddSingleton<ISymbolIdParser, SymbolIdParserService>()
            .TryAddEnumerableSingleton<ISymbolIdParserStrategy, TradingViewSymbolIdParser>()
            ;
    }
}

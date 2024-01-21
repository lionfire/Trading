#nullable enable

namespace LionFire.Trading;

//public class ParsePipeline : Pipeline<SymbolIdParseResult>
//{
//    public ParsePipeline(IServiceProvider serviceProvider)
//    {
//        Add(new TradingViewSymbolIdParser());
//    }
//}

//public class ParsingPipelineBuilder
//{
//    List<Action<SymbolIdParseResult>>
//}



public interface ISymbolInfoResolver
{
    SymbolInfoResolveResult? TryResolve(string symbol);
}

namespace LionFire.Trading.Markets;

public interface ISymbolIdTranslator
{
    SymbolId TranslateFromNative(string symbol);
    string TranslateToNative(SymbolId symbol);
}

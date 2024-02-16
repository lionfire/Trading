using LionFire.Structures;

namespace LionFire.Trading.Scanner;

public class SymbolScannerSettings : IKeyed<string>
{
    public SymbolScannerSettings(string symbol)
    {
        Symbol = symbol;
    }

    public string Symbol { get; init; }
    public string Key => Symbol;
    public static implicit operator KeyValuePair<string, SymbolScannerSettings>(SymbolScannerSettings s) => new(s.Symbol, s);
    public bool Pinned { get; set; }
    public bool PinnedToTop { get; set; }
    public bool PinnedToBottom { get; set; }
}

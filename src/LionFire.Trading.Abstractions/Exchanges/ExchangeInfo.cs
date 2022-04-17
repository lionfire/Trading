using System.Collections.Generic;

namespace LionFire.Trading.Exchanges;

public class ExchangeInfo : IKeyed
{
    public string Key { get; set; }
    public string Name { get; set; }

    public Dictionary<string, ExchangeAreaInfo> Areas { get; } = new();
}


using LionFire.Structures;
using Orleans;

namespace LionFire.Trading.Exchanges;


public class ExchangeAreaInfo : IKeyed
{
    public string Key { get; set; }
    public string Name { get; set; }

    public Type BarType { get; set; }
}




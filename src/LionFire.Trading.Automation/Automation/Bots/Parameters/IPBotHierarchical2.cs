namespace LionFire.Trading.Automation;

public interface IPBotHierarchical2 : IPBot2
{
    IEnumerable<IPBot2>? Children { get; }
    IEnumerable<IPBot2> Descendants => (Children ?? []).Concat((Children ?? []).OfType<IPBotHierarchical2>().SelectMany(c => c.Descendants));
}

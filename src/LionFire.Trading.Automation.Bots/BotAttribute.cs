namespace LionFire.Trading.Automation.Bots;

[System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
public sealed class BotAttribute : Attribute
{
    public BotDirection Direction { get; set; }

}

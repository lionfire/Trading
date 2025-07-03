namespace LionFire.Trading.Automation;

public class BotFaultException : Exception
{
    public BotFaultException(string? message) : base(message) { }
}
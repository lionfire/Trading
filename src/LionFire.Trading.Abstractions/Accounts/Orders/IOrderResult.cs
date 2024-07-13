namespace LionFire.Trading;

public interface IOrderResult
{
    bool Noop { get; }
    bool IsSuccess { get; }
    //bool IsComplete { get; }
    string? Error { get; }
    object? Data { get; }
}


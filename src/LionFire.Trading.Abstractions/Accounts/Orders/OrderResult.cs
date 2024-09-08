namespace LionFire.Trading;

public class OrderResult : IOrderResult

{
    public static IOrderResult NoopSuccess { get; } = new OrderResult { Noop = true, IsSuccess = true };
    public static IOrderResult Success { get; } = new OrderResult { IsSuccess = true };

    public bool IsSuccess { get; init; }

    //public bool IsComplete { get; init; }

    public string? Error { get; init; }

    public object? Data { get; init; }

    public bool Noop { get; init; }

    public List<IOrderResult>? InnerResults { get; set; }
}


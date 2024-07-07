namespace LionFire.Trading;

public class OrderResult : IOrderResult

{
    public bool IsSuccess { get; set; }

    //public bool IsComplete { get; set; }

    public string? Error { get; set; }

    public object? Data { get; set; }
}


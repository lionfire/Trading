#nullable enable
namespace LionFire.Trading;

public static class DateOnlyX // MOVE to LionFire.Base
{
    public static DateTime ToDateTime(this DateOnly date) => date.ToDateTime(new TimeOnly(0, 0, 0));
}


namespace LionFire.Trading.Automation;


public static class DateCoercion
{
    public static DateTimeOffset Coerce(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return default;
        var dt = dateTime.Value;
        if (dt.Kind == DateTimeKind.Unspecified) dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return dt;
    }
}

namespace LionFire.Structures;

public static class DateTimeFormatting
{
    /// <summary>
    /// Date format used in folder names (dots to avoid conflict with hyphen separator)
    /// </summary>
    public const string DateFormat = "yyyy.MM.dd";
    public const string DateTimeFormat = "yyyy.MM.dd_HH-mm-ss";

    /// <summary>
    /// Return a date range in the format "start-end" (no spaces)
    ///
    /// If start or endExclusive is null, it will be written as "UnknownDate"
    ///
    /// If a date has an hour minute and second and millisecond of 0, it will be written as yyyy.MM.dd
    /// Otherwise, it will be written as yyyy.MM.dd_HH-mm-ss
    /// </summary>
    /// <param name="start"></param>
    /// <param name="endExclusive"></param>
    /// <returns></returns>
    public static string ToConciseFileName(DateTimeOffset? start, DateTimeOffset? endExclusive)
    {
        var sb = new StringBuilder();

        // Helper function to format a date
        string FormatDate(DateTimeOffset? date)
        {
            if (date == null)
            {
                return "UnknownDate";
            }

            var dt = date.Value;
            if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0)
            {
                return dt.ToString(DateFormat);
            }
            return dt.ToString(DateTimeFormat);
        }

        // Append start date
        sb.Append(FormatDate(start));

        // Append separator (no spaces - single hyphen)
        sb.Append("-");

        // Append end date
        sb.Append(FormatDate(endExclusive));

        return sb.ToString();
    }

}



namespace DateExt;
public static class JulianDate
{
    public static readonly DateTime JulianBeginDate = new DateTime(1899, 12, 31, 0, 0, 0);

    public static int ToJulianDate(this DateTime inDate)
    {
        var DateDiff = (inDate - new DateTime(inDate.Year, 1, 1).AddDays(-1));
        var yearDiff = inDate.AddDays(DateDiff.Days * -1).Year - JulianBeginDate.Year;
        return yearDiff * 1000 + DateDiff.Days;
    }
    public static int ToJulianTime(this DateTime inDate)
    {
        return inDate.Hour * 10000 + inDate.Minute * 100 + inDate.Second;
    }

    public static DateTime ToDateTime(this int JulianDate, int JulianTime = 0)
    {

        return JulianBeginDate
            .AddYears(JulianDate / 1000)
            .AddDays(JulianDate % 1000)
            .AddHours((int)(JulianTime / 10000))
            .AddMinutes((int)(JulianTime % 10000 / 100))
            .AddSeconds((int)(JulianTime % 100));
    }
}
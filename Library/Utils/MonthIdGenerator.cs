namespace Library.Utils;

public class MonthIdGenerator
{
    public static int GetLastMonthId()
    {
        DateTime lastMonthDate = DateTime.Now.AddMonths(-1);
        return Convert.ToInt32($"{lastMonthDate.Year}{lastMonthDate.Month:D2}");
    }

    public static int GetCurrentMonthId()
    {
        DateTime now = DateTime.Now;
        return Convert.ToInt32($"{now.Year}{now.Month:D2}");
    }

    public static bool CompareMonthIdWithDateTime(int monthId, DateTime dateTime)
    {
        var monthIdFromDateTime = Convert.ToInt32($"{dateTime.Year}{dateTime.Month:D2}");
        return monthId == monthIdFromDateTime;
    }
}
namespace MyApp.Helpers;

/// <summary>
/// Helper class to reduce duplication in recurrence calculation logic
/// </summary>
public static class RecurrenceCalculator
{
    /// <summary>
    /// Calculates the next execution date based on recurrence type
    /// </summary>
    public static DateTime? CalculateNextExecutionDate(DateTime scheduledDate, string recurrenceType, int? recurrenceDay)
    {
        if (recurrenceType == "OneTime")
        {
            return scheduledDate;
        }

        var now = DateTime.UtcNow;
        var nextDate = scheduledDate;

        switch (recurrenceType)
        {
            case "Daily":
                nextDate = now.AddDays(1);
                break;

            case "Weekly":
                var daysUntilNext = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                if (daysUntilNext == 0) daysUntilNext = 7;
                nextDate = now.AddDays(daysUntilNext);
                break;

            case "Monthly":
                if (recurrenceDay.HasValue)
                {
                    var day = Math.Min(recurrenceDay.Value, DateTime.DaysInMonth(now.Year, now.Month));
                    nextDate = new DateTime(now.Year, now.Month, day);
                    if (nextDate <= now)
                    {
                        nextDate = nextDate.AddMonths(1);
                        day = Math.Min(recurrenceDay.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                        nextDate = new DateTime(nextDate.Year, nextDate.Month, day);
                    }
                }
                else
                {
                    nextDate = now.AddMonths(1);
                }
                break;

            case "Quarterly":
                nextDate = now.AddMonths(3);
                break;

            case "Annually":
                nextDate = now.AddYears(1);
                break;
        }

        return nextDate;
    }
}


namespace Library.Utils;

/// <summary>
/// A simple time management class that accumulates hours and minutes.
/// It automatically converts minutes exceeding 60 into hours and provides
/// a friendly, human-readable string representation of the total time.
/// </summary>
public class TimeCalculator
{
    /// <summary>
    /// Gets the total accumulated hours.
    /// </summary>
    public int Hours { get; private set; }
    
    /// <summary>
    /// Gets the total accumulated minutes (less than one full hour).
    /// </summary>
    public int Minutes { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeCalculator"/> class with zero hours and minutes.
    /// </summary>
    public TimeCalculator()
    {
        Hours = 0;
        Minutes = 0;
    }
    
    /// <summary>
    /// Adds the specified hours and minutes to the current time.
    /// This method automatically converts any extra minutes into hours.
    /// </summary>
    /// <param name="hoursToAdd">The number of hours to add.</param>
    /// <param name="minutesToAdd">The number of minutes to add.</param>
    public void Add(int hoursToAdd, int minutesToAdd)
    {
        // Add the minutes, then convert any overflow into hours.
        Minutes += minutesToAdd;
        Hours += hoursToAdd + (Minutes / 60);
        Minutes %= 60;
    }
    
    /// <summary>
    /// Gets the total accumulated time in minutes.
    /// Hours are converted to minutes and added to the remaining minutes.
    /// </summary>
    public int TotalMinutes => Hours * 60 + Minutes;

    /// <summary>
    /// Returns a formatted string representation of the total time.
    /// Only non-zero components are displayed, with proper singular or plural forms,
    /// and combines values with "and" when both hours and minutes are present.
    /// </summary>
    /// <returns>
    /// A friendly string representing the total time (e.g., "2 hours and 15 minutes").
    /// </returns>
    public override string ToString()
    {
        // Handle the case when both hours and minutes are zero.
        if (Hours == 0 && Minutes == 0)
        {
            return "0 minutes";
        }
        
        // Create formatted strings for hours and minutes.
        string hourStr = Hours > 0
            ? $"{Hours} {(Hours == 1 ? "hour" : "hours")}"
            : "";
        string minuteStr = Minutes > 0
            ? $"{Minutes} {(Minutes == 1 ? "minute" : "minutes")}"
            : "";
        
        // Combine the strings in a readable way.
        if (!string.IsNullOrEmpty(hourStr) && !string.IsNullOrEmpty(minuteStr))
        {
            return $"{hourStr} and {minuteStr}";
        }
        else
        {
            return $"{hourStr}{minuteStr}";
        }
    }
}
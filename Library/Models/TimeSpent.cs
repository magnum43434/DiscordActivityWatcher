using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models;

public class TimeSpent
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ulong GuildId { get; set; }
    public DateTime LastActiv { get; set; }
    public string TimeActiv { get; set; }
    public int MinutesActiv { get; set; }
    
    public string ForDisplay(string nickName)
    {
        var danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var danishTime = TimeZoneInfo.ConvertTimeFromUtc(this.LastActiv, danishTimeZone);
        return $"{nickName} has spent {this.TimeActiv} in voice channels.\n\nLast activ: {danishTime}";
    }
}
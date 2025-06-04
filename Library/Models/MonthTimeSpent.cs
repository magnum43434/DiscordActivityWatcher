using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models;

public class MonthTimeSpent
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ulong GuildId { get; set; }
    public int MonthId { get; set; }
    public string TimeActiv { get; set; }
    public int MinutesActiv { get; set; }
}
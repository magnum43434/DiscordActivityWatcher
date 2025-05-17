using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models;

public class User
{
    private Guid _transactionId;
    
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ulong DiscordId { get; set; }
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
    public DateTime LastActiv { get; set; }
    public string TimeActiv { get; set; }
    public int MinutesActiv { get; set; }
    public Guid TransactionId
    {
        get => this._transactionId; set => this._transactionId = value;
    }
    
    public User()
    {
        
    }

    public string ForDisplay(string nickName)
    {
        var danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var danishTime = TimeZoneInfo.ConvertTimeFromUtc(this.LastActiv, danishTimeZone);
        return $"{nickName} has spent {this.TimeActiv} in voice channels.\nLast activ: {danishTime}";
    }
    
    public override string ToString()
    {
        return $"Id: {Id}, DiscordId: {DiscordId}, Username: {Username}, TransactionId: {TransactionId}, LastActiv: {LastActiv}, TimeActiv: {TimeActiv}";
    }
}
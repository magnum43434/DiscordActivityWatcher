using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Library.Enums;

namespace Library.Models;

public class Activity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ulong ChannelId { get; set; }
    public string ChannelName { get; set; }
    public string ChannelType { get; set; }
    public ulong GuildId { get; set; }
    public string GuildName { get; set; }
    public ActivityAction Action { get; set; }
    public DateTime Created { get; set; }
    public string Message { get; set; }
    public Guid TransactionId { get; set; }
    
    public Activity()
    {
        
    }

    public override  string ToString()
    {
        return $"Id: {Id}, UserId: {UserId}, ChannelId: {ChannelId}, ChannelName: {ChannelName}, ChannelType: {ChannelType}, Action: {Action}, Created: {Created}, Message: {Message}, TransactionId: {TransactionId}";
    }
}
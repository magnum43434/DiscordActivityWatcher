using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models;

public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ulong DiscordId { get; set; }
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
    public Guid TransactionId { get; set; }
    
    public override string ToString()
    {
        return $"Id: {Id}, DiscordId: {DiscordId}, Username: {Username}, TransactionId: {TransactionId}";
    }
}
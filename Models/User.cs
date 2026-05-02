using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string FirebaseUid { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? DisplayName { get; set; }
    
    public string? PhotoUrl { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

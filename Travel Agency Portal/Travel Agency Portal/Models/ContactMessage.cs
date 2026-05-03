using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class ContactMessage
{
    [Key]
    public int C_Id { get; set; }

    [Required]
    [StringLength(100)]
    public string C_Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string C_Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string C_Email { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string C_Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string C_Message { get; set; } = string.Empty;

    public int? U_Id { get; set; }

    public bool C_IsRead { get; set; }

    public bool C_IsArchived { get; set; }

    public DateTime C_CreatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class Users
{
    [Key]
    public int U_Id { get; set; }

    [Required]
    [StringLength(100)]
    public string U_Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string U_Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string U_Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string U_Username { get; set; } = string.Empty;

    public string? U_Phone { get; set; }

    [Required]
    public string U_Password { get; set; } = string.Empty;

    [Required]
    public string U_RepeatPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string U_Type { get; set; } = "user";
}

public class UserProfile
{
    public int U_Id { get; set; }
    public string U_Name { get; set; } = string.Empty;
    public string U_Surname { get; set; } = string.Empty;
    public string U_Email { get; set; } = string.Empty;
    public string U_Username { get; set; } = string.Empty;
    public string? U_Phone { get; set; }
    public string U_Type { get; set; } = "user";
}

public class UpdateUserProfileRequest
{
    [Required]
    [StringLength(100)]
    public string U_Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string U_Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string U_Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string U_Username { get; set; } = string.Empty;

    public string? U_Phone { get; set; }
}

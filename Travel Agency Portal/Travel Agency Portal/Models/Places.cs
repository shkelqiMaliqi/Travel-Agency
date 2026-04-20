using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class Place
{
    [Key]
    public int Place_Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Place_Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Place_Description { get; set; } = string.Empty;

    [Url]
    public string? Place_Url { get; set; }
}

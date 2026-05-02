using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class Hotel
{
    [Key]
    public int Hotel_Id { get; set; }

    [Required]
    public int Place_Id { get; set; }

    public string? Place_Name { get; set; }

    [Required]
    [StringLength(150)]
    public string Hotel_Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Hotel_Description { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Hotel_Stars { get; set; } = 3;

    [Url]
    public string? Hotel_Url { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Dtos;

public class PackageRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Destination is required.")]
    public int Place_Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Hotel is required.")]
    public int Hotel_Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Package_Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1200)]
    public string Package_Description { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Price_Per_Person { get; set; }

    [Required]
    public DateTime Start_Date { get; set; }

    [Required]
    public DateTime End_Date { get; set; }

    [Range(0, 10000)]
    public int Available_Seats { get; set; }

    [Url]
    public string? Package_Url { get; set; }
}

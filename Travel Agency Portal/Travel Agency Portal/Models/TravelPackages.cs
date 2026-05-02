using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class TravelPackage
{
    [Key]
    public int Package_Id { get; set; }

    [Required]
    public int Place_Id { get; set; }

    [Required]
    public int Hotel_Id { get; set; }

    public string? Place_Name { get; set; }
    public string? Hotel_Name { get; set; }

    [Required]
    [StringLength(150)]
    public string Package_Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1200)]
    public string Package_Description { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Price_Per_Person { get; set; }

    [DataType(DataType.Date)]
    public DateTime Start_Date { get; set; }

    [DataType(DataType.Date)]
    public DateTime End_Date { get; set; }

    [Range(0, 10000)]
    public int Available_Seats { get; set; }

    [Url]
    public string? Package_Url { get; set; }
}

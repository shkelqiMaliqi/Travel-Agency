using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class Booking
{
    [Key]
    public int Booking_Id { get; set; }

    public int Package_Id { get; set; }
    public int U_Id { get; set; }
    public string? Customer_Name { get; set; }
    public string? Customer_Email { get; set; }
    public string? Customer_Phone { get; set; }
    public string? Package_Name { get; set; }
    public string? Place_Name { get; set; }
    public string? Hotel_Name { get; set; }
    public int Travelers { get; set; }
    public decimal Total_Price { get; set; }
    public string Booking_Status { get; set; } = "Pending";
    public DateTime Booking_Date { get; set; }
}

public class CreateBookingRequest
{
    [Required]
    public int Package_Id { get; set; }

    [Range(1, 20)]
    public int Travelers { get; set; } = 1;
}

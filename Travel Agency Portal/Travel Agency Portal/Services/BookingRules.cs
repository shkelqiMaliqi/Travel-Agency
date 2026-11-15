using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public static class BookingRules
{
    public static string? ValidateRequest(CreateBookingRequest request) =>
        request.Package_Id <= 0 ? "Choose a valid package." :
        request.Travelers is < 1 or > 20 ? "Travelers must be between 1 and 20." : null;

    public static string? ValidateAvailability(int travelers, int seats, DateTime travelDate, DateTime start, DateTime end) =>
        travelDate.Date < start.Date || travelDate.Date > end.Date
            ? "Travel date must be between the package start and end dates." :
        seats < travelers ? "Not enough seats are available for this package." : null;

    public static string? ValidateTransition(string current, string next, bool userCancellation) =>
        next is not ("Pending" or "Confirmed" or "Cancelled")
            ? "Status must be Pending, Confirmed, or Cancelled." :
        userCancellation && current != "Pending"
            ? "Only pending bookings can be cancelled by the user." : null;
}

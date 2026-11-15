using System.Data.SqlClient;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Travel_Agency_Portal.Tests;

public sealed class ApiBusinessTests(SqlApiFixture fixture) : IClassFixture<SqlApiFixture>
{
    private static async Task<JsonElement> Data(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data").Clone();
    }

    [Fact]
    public async Task DestinationCrud_InvalidatesFilteredAndDetailCaches()
    {
        using var admin = fixture.Client("admin");
        using var guest = fixture.Client();
        var name = "Crud" + Guid.NewGuid().ToString("N");
        var payload = new { place_Name = name, place_Description = "Created in an isolated integration database" };
        var before = await Data(await guest.GetAsync("/api/v1/places?search=" + name));
        Assert.Empty(before.EnumerateArray());
        Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/v1/places", payload)).StatusCode);
        var created = await Data(await guest.GetAsync("/api/v1/places?search=" + name));
        var item = Assert.Single(created.EnumerateArray());
        var id = item.GetProperty("place_Id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await guest.GetAsync($"/api/v1/places/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync($"/api/v1/places/{id}", new
        { place_Name = name + "Edited", place_Description = "Updated description" })).StatusCode);
        var updated = await Data(await guest.GetAsync($"/api/v1/places/{id}"));
        Assert.Equal(name + "Edited", updated.GetProperty("place_Name").GetString());
        Assert.Equal(HttpStatusCode.OK, (await admin.DeleteAsync($"/api/v1/places/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await guest.GetAsync($"/api/v1/places/{id}")).StatusCode);
        Assert.Empty((await Data(await guest.GetAsync("/api/v1/places?search=" + name))).EnumerateArray());
    }

    [Theory]
    [InlineData(null, "GET", "/api/v1/users", 401)]
    [InlineData("user", "GET", "/api/v1/users", 403)]
    [InlineData("auditor", "GET", "/api/v1/users", 200)]
    [InlineData("admin", "GET", "/api/v1/users", 200)]
    [InlineData("auditor", "DELETE", "/api/v1/places/1", 403)]
    [InlineData("user", "DELETE", "/api/v1/places/1", 403)]
    [InlineData("user", "GET", "/api/v1/users/2", 403)]
    public async Task RolePermissions_AreEnforcedByJwtMiddleware(string? role, string method, string path, int expected)
    {
        using var client = fixture.Client(role);
        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));
        Assert.Equal(expected, (int)response.StatusCode);
    }

    [Fact]
    public async Task InvalidRegistration_ReturnsStructuredValidationErrors()
    {
        using var client = fixture.Client();
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.Contains("Validation", json.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Booking_RejectsOverbookingInvalidDatesAndZeroTravelers_WithoutChangingSeats()
    {
        var id = await fixture.NewPackageAsync(2);
        using var client = fixture.Client("user");
        foreach (var body in new[]
        {
            new { package_Id = id, travelers = 3, travel_Date = "2030-01-02" },
            new { package_Id = id, travelers = 1, travel_Date = "2030-02-01" },
            new { package_Id = id, travelers = 0, travel_Date = "2030-01-02" }
        }) Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/bookings", body)).StatusCode);
        Assert.Equal(2, await Seats(id));
    }

    [Fact]
    public async Task ConcurrentBookings_CannotOversell_AndCancellationRestoresSeatsOnce()
    {
        var id = await fixture.NewPackageAsync(2);
        using var first = fixture.Client("user");
        using var second = fixture.Client("user");
        // Warm the detail cache before booking; the next read must show updated availability.
        await first.GetAsync($"/api/v1/packages/{id}");
        var body = new { package_Id = id, travelers = 2, travel_Date = "2030-01-03" };
        var responses = await Task.WhenAll(first.PostAsJsonAsync("/api/v1/bookings", body), second.PostAsJsonAsync("/api/v1/bookings", body));
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Equal(0, await Seats(id));
        var detail = await Data(await first.GetAsync($"/api/v1/packages/{id}"));
        Assert.Equal(0, detail.GetProperty("available_Seats").GetInt32());
        var booking = await fixture.ScalarAsync<int>("SELECT Booking_Id FROM dbo.Bookings WHERE Package_Id=@Id", new SqlParameter("@Id", id));
        using var stranger = fixture.Client("user", 2);
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.GetAsync($"/api/v1/bookings/{booking}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.PutAsJsonAsync($"/api/v1/bookings/{booking}/cancel", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await first.PutAsJsonAsync($"/api/v1/bookings/{booking}/cancel", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await first.PutAsJsonAsync($"/api/v1/bookings/{booking}/cancel", new { })).StatusCode);
        Assert.Equal(2, await Seats(id));
    }

    [Fact]
    public async Task ConfirmedBooking_CannotBeCancelledByCustomer()
    {
        var id = await fixture.NewPackageAsync(4);
        using var user = fixture.Client("user");
        using var admin = fixture.Client("admin");
        Assert.Equal(HttpStatusCode.Created, (await user.PostAsJsonAsync("/api/v1/bookings", new { package_Id = id, travelers = 1 })).StatusCode);
        var booking = await fixture.ScalarAsync<int>("SELECT Booking_Id FROM dbo.Bookings WHERE Package_Id=@Id", new SqlParameter("@Id", id));
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync($"/api/v1/bookings/{booking}/status", "Confirmed")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await user.PutAsJsonAsync($"/api/v1/bookings/{booking}/cancel", new { })).StatusCode);
        Assert.Equal(3, await Seats(id));
    }

    [Fact]
    public async Task PasswordRecovery_HidesAccountExistence_StoresOnlyHash_AndPreventsReplay()
    {
        using var client = fixture.Client();
        var email = "admin@travelagency.com";
        var existing = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        var unknown = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "absent@example.com" });
        Assert.Equal(HttpStatusCode.OK, existing.StatusCode);
        Assert.Equal(await existing.Content.ReadAsStringAsync(), await unknown.Content.ReadAsStringAsync());
        var code = fixture.Delivery.Codes[email];
        Assert.Equal(64, code.Length);
        Assert.DoesNotContain(code, await existing.Content.ReadAsStringAsync());
        var stored = await fixture.ScalarAsync<string>("SELECT TOP 1 Reset_Code_Hash FROM dbo.Password_Reset_Codes WHERE U_Email=@Email ORDER BY Reset_Id DESC", new SqlParameter("@Email", email));
        Assert.NotEqual(code, stored);
        var reset = new { email, resetCode = code, newPassword = "NewSafePassword123!", confirmPassword = "NewSafePassword123!" };
        var results = await Task.WhenAll(client.PostAsJsonAsync("/api/v1/auth/reset-password", reset), client.PostAsJsonAsync("/api/v1/auth/reset-password", reset));
        Assert.Single(results, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(results, r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/login", new { userNameOrEmail = email, password = "NewSafePassword123!" })).StatusCode);
    }

    private Task<int> Seats(int id) => fixture.ScalarAsync<int>("SELECT Available_Seats FROM dbo.Travel_Packages WHERE Package_Id=@Id", new SqlParameter("@Id", id));

    [Fact]
    public async Task HotelAndPackageCrud_ValidateRelationsAndRefreshCatalog()
    {
        using var admin = fixture.Client("admin");
        using var guest = fixture.Client();
        var placeResponse = await admin.PostAsJsonAsync("/api/v1/places", new
        { place_Name = "Relation " + Guid.NewGuid().ToString("N"), place_Description = "CRUD integration fixture" });
        Assert.Equal(HttpStatusCode.Created, placeResponse.StatusCode);
        Assert.NotNull(placeResponse.Headers.Location);
        var place = (await Data(placeResponse)).GetProperty("id").GetInt32();
        var hotelPayload = new { place_Id = place, hotel_Name = "Test Hotel", hotel_Description = "Fixture", hotel_Stars = 4 };
        var hotelResponse = await admin.PostAsJsonAsync("/api/v1/hotels", hotelPayload);
        Assert.Equal(HttpStatusCode.Created, hotelResponse.StatusCode);
        var hotel = (await Data(hotelResponse)).GetProperty("id").GetInt32();
        var name = "Package " + Guid.NewGuid().ToString("N");
        object Package(int placeId, string endDate = "2030-01-10", int seats = 5) => new
        {
            place_Id = placeId,
            hotel_Id = hotel,
            package_Name = name,
            package_Description = "Fixture",
            price_Per_Person = 100,
            start_Date = "2030-01-01",
            end_Date = endDate,
            available_Seats = seats
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/v1/packages", Package(place, "2029-01-01"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/v1/packages", Package(place + 100000))).StatusCode);
        await guest.GetAsync("/api/v1/packages?search=" + Uri.EscapeDataString(name));
        var response = await admin.PostAsJsonAsync("/api/v1/packages", Package(place));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await Data(response)).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await guest.GetAsync(response.Headers.Location)).StatusCode);
        Assert.Single((await Data(await guest.GetAsync("/api/v1/packages?search=" + Uri.EscapeDataString(name)))).EnumerateArray());
        Assert.Equal(HttpStatusCode.Conflict, (await admin.DeleteAsync($"/api/v1/hotels/{hotel}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.DeleteAsync($"/api/v1/places/{place}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync($"/api/v1/packages/{id}", Package(place, seats: 8))).StatusCode);
        Assert.Equal(8, (await Data(await guest.GetAsync($"/api/v1/packages/{id}"))).GetProperty("available_Seats").GetInt32());
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync($"/api/v1/hotels/{hotel}", new
        { place_Id = place, hotel_Name = "Edited Hotel", hotel_Description = "Fixture", hotel_Stars = 5 })).StatusCode);
        Assert.Equal("Edited Hotel", (await Data(await guest.GetAsync($"/api/v1/packages/{id}"))).GetProperty("hotel_Name").GetString());
        Assert.Equal(HttpStatusCode.OK, (await admin.DeleteAsync($"/api/v1/packages/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await guest.GetAsync($"/api/v1/packages/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.DeleteAsync($"/api/v1/hotels/{hotel}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.DeleteAsync($"/api/v1/places/{place}")).StatusCode);
    }

    [Fact]
    public async Task ExpiredResetToken_CannotChangePassword()
    {
        using var client = fixture.Client();
        var email = "auditor@travelagency.com";
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email })).StatusCode);
        var code = fixture.Delivery.Codes[email];
        await fixture.ScalarAsync<int>("UPDATE dbo.Password_Reset_Codes SET Expires_At = DATEADD(minute,-1,SYSUTCDATETIME()) WHERE U_Email=@Email; SELECT @@ROWCOUNT;", new SqlParameter("@Email", email));
        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        { email, resetCode = code, newPassword = "DifferentSafe123!", confirmPassword = "DifferentSafe123!" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Registration_CannotAssignAnAdminRole_AndLoginUsesTheStoredPasswordHash()
    {
        using var client = fixture.Client();
        var username = "user" + Guid.NewGuid().ToString("N");
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            u_Name = "Test",
            u_Surname = "Traveler",
            u_Email = username + "@example.com",
            u_Username = username,
            password = "StrongUserPassword123!",
            confirmPassword = "StrongUserPassword123!",
            u_Type = "admin"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("user", (await Data(response)).GetProperty("role").GetString());
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { userNameOrEmail = username, password = "StrongUserPassword123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal("user", (await Data(login)).GetProperty("role").GetString());
    }

    [Fact]
    public async Task RepeatedPasswordResetRequests_AreRateLimited()
    {
        // An independent host gives this rate-limit test its own counter.
        await using var factory = fixture.Factory.WithWebHostBuilder(_ => { });
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "absent@example.com" })).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "absent@example.com" })).StatusCode);
    }
}

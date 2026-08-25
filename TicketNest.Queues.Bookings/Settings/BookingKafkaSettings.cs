using TicketNest.Shared.Guard;

namespace TicketNest.Queues.Bookings.Settings;

public class BookingKafkaSettings
{
    public string BaseUrl { get; }
    public string? Login { get; }
    public string? Password { get; }

    public BookingKafkaSettings(string baseUrl, string? login, string? password)
    {
        Ensure.NotNullOrEmpty(baseUrl, nameof(baseUrl));

        BaseUrl = baseUrl;
        Login = login;
        Password = password;
    }
}
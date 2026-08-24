using TicketNest.Shared.Guard;

namespace TicketNest.Queues.Events.Settings;

public class EventsKafkaSettings
{
    public string BaseUrl { get; }
    public string Login { get; }
    public string Password { get; }

    public EventsKafkaSettings(string baseUrl, string login, string password)
    {
        Ensure.NotNullOrEmpty(baseUrl, nameof(baseUrl));
        Ensure.NotNullOrEmpty(login, nameof(login));
        Ensure.NotNullOrEmpty(password, nameof(password));

        BaseUrl = baseUrl;
        Login = login;
        Password = password;
    }
}
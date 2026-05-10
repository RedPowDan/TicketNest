using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TicketNest.Api.Infrastructure;

internal static class JsonSettingsConfigurator
{
    public static void ConfigureSettings(JsonSerializerSettings settings)
    {
        var strategy = new CamelCaseNamingStrategy();
        settings.Converters.Add(new StringEnumConverter(strategy));
        settings.NullValueHandling = NullValueHandling.Ignore;
    }
}
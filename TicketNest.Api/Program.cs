using Microsoft.AspNetCore;

namespace TicketNest.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateWebHostBuilder(args).Build();

        await host.RunAsync();
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder<Startup>(args);
}
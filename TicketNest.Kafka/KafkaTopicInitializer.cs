using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace TicketNest.Kafka;

public static class KafkaTopicInitializer
{
    public static void EnsureTopicsCreated(string bootstrapServers, string? login, string? password, params string[] topics)
    {
        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            return;
        }

        try
        {
            EnsureTopicsCreatedAsync(bootstrapServers, login, password, topics)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kafka topic initialization skipped: {ex.Message}");
        }
    }

    private static async Task EnsureTopicsCreatedAsync(string bootstrapServers, string? login, string? password, string[] topics)
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers,
        };

        if (!string.IsNullOrWhiteSpace(login))
        {
            adminConfig.SaslUsername = login;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            adminConfig.SaslPassword = password;
        }

        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        var specifications = new List<TopicSpecification>(topics.Length);
        foreach (var topic in topics)
        {
            specifications.Add(new TopicSpecification
            {
                Name = topic,
                NumPartitions = 2,
                ReplicationFactor = 1,
            });
        }

        try
        {
            await adminClient.CreateTopicsAsync(
                specifications,
                new CreateTopicsOptions { OperationTimeout = TimeSpan.FromSeconds(30) });
        }
        catch (CreateTopicsException ex)
        {
            foreach (var result in ex.Results)
            {
                if (result.Error.Code != ErrorCode.TopicAlreadyExists)
                {
                    throw;
                }
            }
        }
    }
}

using TicketNest.Events.IntegrationTests.Infrastructure;

namespace TicketNest.Events.IntegrationTests;

[CollectionDefinition("Redis")]
public sealed class RedisCollection : ICollectionFixture<RedisContainerFixture>
{
}

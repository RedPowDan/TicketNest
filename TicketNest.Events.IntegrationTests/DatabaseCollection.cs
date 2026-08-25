using TicketNest.Events.IntegrationTests.Infrastructure;

namespace TicketNest.Events.IntegrationTests;

[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
}

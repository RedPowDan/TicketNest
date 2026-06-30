using TicketNest.IntegrationTests.Infrastructure;

namespace TicketNest.IntegrationTests;

[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
}

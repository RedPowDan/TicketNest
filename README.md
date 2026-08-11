# TicketNest

Бэкенд-сервис для управления событиями и бронированиями (билетная система).

## Технологии

- **.NET 9**, **ASP.NET Core**
- **PostgreSQL** (Entity Framework Core + Npgsql)
- **Clean Architecture** (Domain → Application → Presentation)
- **Domain-Driven Design** (Aggregate Roots, Domain Services, Value Objects)
- **Result pattern** (`Result<TValue, TError>`) для обработки ошибок без исключений
- **CQRS**-like разделение команд и запросов на уровне сервисов
- **SemaphoreSlim** для защиты от состояний гонки при бронировании
- **Background Service** для асинхронного подтверждения броней
- **FluentAssertions** + **NSubstitute** (тесты)
- **NUnit** (UnitTests), **xUnit** (IntegrationTests)
- **Testcontainers.PostgreSql** (интеграционные тесты с реальной БД)

## Функционал

### Events (`/events`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/events` | Список событий (пагинация, фильтрация по названию и датам) |
| GET | `/events/{id}` | Получить событие по GUID |
| POST | `/events` | Создать новое событие |
| PUT | `/events/{id}` | Обновить событие |
| DELETE | `/events/{id}` | Удалить событие |
| POST | `/events/{id}/book` | Создать бронирование (202 Accepted / 409 Conflict) |

### Bookings (`/bookings`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/bookings/{id}` | Получить информацию о бронировании |

### Модель Event

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | `Guid` | Уникальный идентификатор |
| `Title` | `string` | Название |
| `Description` | `string?` | Описание (необязательно) |
| `StartAt` | `DateTime` | Дата и время начала (UTC) |
| `EndAt` | `DateTime` | Дата и время окончания (UTC) |
| `TotalSeats` | `int` | Общее количество мест |
| `AvailableSeats` | `int` | Свободных мест на данный момент |

### Модель Booking

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | `Guid` | Уникальный идентификатор |
| `EventId` | `Guid` | Идентификатор события |
| `Status` | `BookingStatus` | Текущий статус |
| `CreatedAt` | `DateTime` | Дата и время создания (UTC) |
| `ProcessedAt` | `DateTime?` | Дата и время обработки |

Статусы: `Pending` → `Confirmed` | `Rejected`

### Защита от овербукинга

`BookingService` использует `SemaphoreSlim(1,1)` для сериализации запросов на бронирование. Каждый запрос атомарно проверяет места (`TryReserveSeats()`), сохраняет бронь и событие, затем отпускает семафор. При сбое подтверждения броня переводится в `Rejected`, а место восстанавливается (`ReleaseSeats()`).

### Фоновая обработка

После создания брони сообщение попадает в очередь. `BookingConfirmationBackgroundService` в бесконечном цикле читает очередь, эмулирует обращение к внешней билетной системе и подтверждает бронь. При ошибке — компенсация (Reject + ReleaseSeats).

## Архитектура

```
TicketNest/
├── TicketNest.Api                     # Presentation
│   ├── Controllers/V1/                # Тонкие контроллеры
│   ├── Models/V1/                     # DTO (запросы/ответы)
│   ├── Mappers/                       # Domain → DTO
│   ├── Middlewares/                   # Exception handling
│   ├── Exceptions/                    # API-исключения
│   ├── Startup.cs                     # Composition root (DI)
│   └── Program.cs
│
├── TicketNest.Application             # Application
│   ├── Services/
│   │   ├── Events/                    # EventService (use cases)
│   │   └── Bookings/                  # BookingService (use cases)
│   └── BackgroundServices/            # Фоновые обработчики
│
├── TicketNest.Domain                  # Domain (core)
│   ├── Models/
│   │   ├── Events/Event.cs            # Aggregate root
│   │   ├── Bookings/Booking.cs        # Aggregate root
│   │   ├── Queue/QueueMessage.cs      # Value object
│   │   └── Error.cs                   # Error model
│   ├── Repositories/                  # Ports (interfaces)
│   ├── Services/Bookings/             # Domain services
│   ├── Filters/                       # Specification objects
│   ├── Pagination/                    # Pagination primitives
│   └── Constants/                     # Domain enums
│
├── TicketNest.DataAccess.Events       # Infrastructure (RDBMS)
│   ├── Implementations/               # Repository implementations
│   ├── Models/                        # EF Core persistence models
│   ├── Mappers/                       # Domain ↔ Persistence
│   ├── DbContext/                     # EventsDbContext
│   └── Migrations/                    # EF Core migrations
│
├── TicketNest.DataAccess.Queue        # Infrastructure (queue)
│   └── Implementations/               # In-memory queue
│
├── TicketNest.Shared                  # Shared kernel
│   ├── Guard/                         # Guard clauses
│   └── Objects/                       # Result pattern
│
├── TicketNest.UnitTests               # NUnit
│   └── ...
│
└── TicketNest.IntegrationTests        # xUnit + Testcontainers
    └── ...
```

### Направление зависимостей

```
Shared → Domain → Application ← Infrastructure
                        ↑            |
                        └── Api ─────┘
```

- **Shared** — toolkit без зависимостей (Guard, Result pattern)
- **Domain** — бизнес-правила, порты репозиториев, доменные службы. Зависит только от Shared
- **Application** — use cases, оркестрация, фоновые задачи. Зависит от Domain
- **Infrastructure** (DataAccess.\*) — реализация портов (EF Core, очереди). Зависит от Application
- **Api** — composition root, DI, middleware, DTO. Зависит от Application и Infrastructure

### Ключевые принципы

- Порты (интерфейсы репозиториев) объявлены в Domain — DDD-подход, где доменные службы используют абстракции для поддержания инвариантов
- Контроллеры не содержат бизнес-логики, только маппинг и делегирование сервисам
- Инфраструктурные детали (EF Core, ORM) не протекают в Application или Domain
- DI-регистрация каждого слоя через extension-методы

## Требования

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PostgreSQL 14+** для запуска приложения
- **Docker** для интеграционных тестов

## Установка и запуск

1. Настройте строку подключения в `TicketNest.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "EventsDbConnection": "Host=localhost;Port=5432;Database=eventapi;Username=postgres;Password=postgres"
  }
}
```

2. Запустите:

```bash
dotnet restore
dotnet run --project TicketNest.Api
```

Схема БД создаётся автоматически через EF Core EnsureCreated.

## Тесты

### Unit-тесты

```bash
dotnet test TicketNest.UnitTests
```

Изолированы через EF Core InMemory и NSubstitute. PostgreSQL не требуется. 133 теста.

### Интеграционные тесты

```bash
dotnet test TicketNest.IntegrationTests
```

Требуют **Docker**. Testcontainers автоматически поднимает PostgreSQL-контейнер, создаёт схему и сбрасывает данные между тестами. 19 тестов.

### CI

GitHub Actions (`BuildBackend.yml`): сборка → unit-тесты + integration-тесты (параллельно).

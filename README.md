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

| Метод | Эндпоинт | Описание | Доступ |
|-------|----------|----------|--------|
| GET | `/events` | Список событий (пагинация, фильтрация по названию и датам) | Все |
| GET | `/events/{id}` | Получить событие по GUID | Все |
| POST | `/events` | Создать новое событие | **Только Admin** |
| PUT | `/events/{id}` | Обновить событие | **Только Admin** |
| DELETE | `/events/{id}` | Удалить событие | **Только Admin** |
| POST | `/events/{id}/book` | Создать бронирование (202 Accepted / 409 Conflict) | Аутентиф. пользователь |

### Bookings (`/bookings`)

| Метод | Эндпоинт | Описание | Доступ |
|-------|----------|----------|--------|
| GET | `/bookings/{id}` | Получить информацию о бронировании | Аутентиф. пользователь |
| DELETE | `/bookings/{id}` | Отменить бронирование (204 No Content) | Владелец брони или **Admin** |

### Auth (`/auth`)

| Метод | Эндпоинт | Описание | Доступ |
|-------|----------|----------|--------|
| POST | `/auth/register` | Регистрация пользователя (роль `User` по умолчанию, можно передать `Admin`). 204 No Content / 400 | Анонимно |
| POST | `/auth/login` | Вход по логину и паролю, возвращает JWT-токен. 200 OK / 400 | Анонимно |

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

## Аутентификация и авторизация

Приложение использует JWT Bearer-токены. Пользователь получает токен через `POST /auth/login` и далее передаёт его в заголовке `Authorization: Bearer <token>`.

### Ролевая модель

Сущность `User` содержит поля `Login`, `PasswordHash` (хеш пароля, SHA-256) и `Role`:
- `User` (0) — обычный пользователь, назначается по умолчанию при регистрации;
- `Admin` (1) — администратор.

### Разграничение прав

| Действие | Эндпоинт | User | Admin |
|----------|----------|------|-------|
| Регистрация / вход | `/auth/register`, `/auth/login` | ✅ (анонимно) | ✅ (анонимно) |
| Просмотр событий | `GET /events`, `GET /events/{id}` | ✅ | ✅ |
| Бронирование | `POST /events/{id}/book` | ✅ | ✅ |
| Просмотр брони | `GET /bookings/{id}` | ✅ | ✅ |
| Отмена брони | `DELETE /bookings/{id}` | ✅ только **свою** | ✅ любую |
| Создание/изменение/удаление событий | `POST/PUT/DELETE /events` | ❌ (403) | ✅ |

Правило «свою бронь может отменить любой пользователь, чужую — только администратор» инкапсулировано в доменном методе `Booking.CanCancel(User)` и покрыто юнит-тестами. При нарушении прав возвращается **403 Forbidden**; при отсутствии токена — **401 Unauthorized**.

Отсутствие или невалидный токен:
- эндпоинты `/auth/*` — доступны без токена;
- защищённые эндпоинты без токена — **401 Unauthorized**;
- эндпоинты, требующие роль `Admin`, для обычного пользователя — **403 Forbidden**.

### Получение JWT-токена через Swagger

1. Запустите приложение (`dotnet run --project TicketNest.Api`) и откройте Swagger UI: `http://localhost:5000/swagger` (или `https://localhost:5001/swagger`).
2. Выполните `POST /auth/login`, передав в теле логин и пароль:
   ```json
   {
     "login": "alice",
     "password": "secret"
   }
   ```
   В ответе (`200 OK`) вернётся JWT-токен:
   ```json
   { "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
   ```
   > Если пользователя ещё нет — сначала зарегистрируйте его через `POST /auth/register` (укажите `"role": 1`, чтобы создать администратора).
3. В верхней правой части Swagger UI нажмите кнопку **Authorize** 🔒.
4. В поле ввода укажите токен в формате `Bearer <token>` (например, `Bearer eyJhbGciOi...`) и нажмите **Authorize**, затем **Close**.
5. Теперь все защищённые запросы из Swagger UI автоматически отправляются с заголовком `Authorization: Bearer <token>`.

### Настройка JWT

Параметры токенов задаются в секции `Jwt` файла `TicketNest.Api/appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "replace-with-a-long-random-secret-key-32bytes-minimum",
    "Issuer": "TicketNest",
    "Audience": "TicketNest.Api",
    "LifetimeMinutes": 60
  }
}
```

- `Secret` — секретный ключ для подписи токена (HS256). Должен быть достаточно длинным (рекомендуется ≥ 32 байт / 256 бит) и содержать криптографически стойкий случайный набор символов.
- `Issuer` / `Audience` — издатель и аудитория токена, проверяются при валидации.
- `LifetimeMinutes` — время жизни токена.

> ⚠️ **Продакшн:** никогда не используйте значение `Secret` по умолчанию из репозитория. Сгенерируйте сильный секрет (например, `openssl rand -base64 48`) и передавайте его через переменные окружения или секреты CI/CD, а не коммитьте в `appsettings.json`. Для реальных сценариев рассмотрите асимметричные ключи (RS256) и хранение секретов в HashiCorp Vault / Azure Key Vault / AWS Secrets Manager.

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

После запуска Swagger UI доступен по адресу `http://localhost:5000/swagger` (или `https://localhost:5001/swagger`). Инструкция по авторизации в Swagger — в разделе [Аутентификация и авторизация](#аутентификация-и-авторизация).

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

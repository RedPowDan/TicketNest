# TicketNest

Распределённая билетная система (бронирование событий), состоящая из **трёх независимых микросервисов**, обменивающихся данными через **Kafka**. Каждый сервис построен по принципам чистой архитектуры и имеет собственную базу данных.

## Сервисы

| Сервис | Проект | Назначение | База данных |
|--------|--------|------------|-------------|
| **Users / Auth** | `TicketNest.Auth.Api` | Регистрация, логин, выпуск JWT | `auth` (`AuthDbContext`) |
| **Events** | `TicketNest.Events.Api` | Управление событиями, учёт свободных мест | `events` (`EventsDbContext`) |
| **Bookings** | `TicketNest.Bookings.Api` | Создание/отмена броней | `bookings` (`BookingsDbContext`) |

Сервисы **не вызывают друг друга напрямую по HTTP** — вся межсервисная коммуникация идёт через Kafka.

## Архитектура

Каждый сервис разделён на слои чистой архитектуры:

```
TicketNest.<Service>.Api        # Presentation — контроллеры, DTO, middleware, Swagger, Startup
TicketNest.Application.<Service> # Application — сценарии использования, фоновые сервисы
TicketNest.Domain.<Service>      # Domain — сущности, агрегаты, порты репозиториев, доменные сервисы
TicketNest.DataAccess.<Service>  # Infrastructure — EF Core, реализации репозиториев, миграции
```

Общие/разделяемые проекты:

```
TicketNest.Contracts   # Общий контракт: имена топиков (KafkaTopics) и DTO сообщений (Messages)
TicketNest.Kafka       # Транспорт Kafka: producer/consumer gateway + создание топиков при старте
TicketNest.Queues.*    # Адаптеры Kafka для конкретного сервиса (Bookings / Events)
TicketNest.Shared      # Общий каркас: Result-паттерн, Guard, TokenUser
TicketNest.Infrastructure # Реализация JWT (генерация/валидация), хеширование паролей
```

Направление зависимостей: `Shared → Domain → Application → Infrastructure → Api`, при этом `Contracts` и `Kafka` являются точками интеграции, не зависящими от бизнес-логики.

## Обмен сообщениями через Kafka

Общий контракт вынесен в `TicketNest.Contracts`:

- `KafkaTopics.BookingTopic` — топик бронирований (`"BookingTopic"`)
- `KafkaTopics.EventTopic` — топик событий (`"EventTopic"`)

Поток бронирования (choreography):

1. `Bookings.Api` сохраняет бронь в свою БД и поднимает доменное событие `BookingCreated`.
2. Через **Outbox** (`TicketNest.DataAccess.Bookings`) событие сначала сохраняется в таблицу БД, а затем публикуется в Kafka (гарантия «сохранили → опубликовали»).
3. `Bookings.Queues` публикует `BookingCreatedMessage` в `BookingTopic`.
4. `Events.Api` (`BookingCreatedBackgroundService`) потребляет `BookingTopic`, резервирует места (`EventReserveService.Reserve` уменьшает `AvailableSeats`) и публикует `BookingApprovedMessage`/`BookingRejectedMessage` в `EventTopic`.
5. `Bookings.Api` (`BookingConfirmationBackgroundService`) потребляет `EventTopic` и подтверждает/отклоняет бронь.

Для отмены публикуется `BookingCancelledMessage` (места восстанавливаются).

### Создание топиков при старте

`TicketNest.Kafka.KafkaTopicInitializer` при запуске каждого Kafka-сервиса через `AdminClient` создаёт топики `BookingTopic` и `EventTopic`, если они ещё не существуют. Система работает «из коробки» даже на пустом брокере.

## Аутентификация и авторизация (JWT)

- Сервис **Auth** выпускает JWT (HS256) через `TicketNest.Infrastructure.JwtTokenGenerator`.
- Сервисы **Events** и **Bookings** валидируют тот же токен, используя **одинаковые** `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience` (общий секрет/издатель/аудитория).
- Эндпоинты управления событиями (`POST/PUT/DELETE /events`) доступны только роли `Admin` (`[Authorize(Roles = "Admin")]`) — возвращается **403** для остальных.
- Эндпоинты бронирований требуют аутентификации (`[Authorize]`) — **401** без токена.
- Анонимны только `POST /auth/register` и `POST /auth/login`.

Роли: `User` (0) — по умолчанию, `Admin` (1).

## Требования

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PostgreSQL 14+** (отдельная БД для каждого сервиса)
- **Kafka** (для межсервисного обмена)
- **Docker** (для запуска в контейнерах / интеграционных тестов)

## Запуск локально

1. Поднимите PostgreSQL (3 БД: `auth`, `events`, `bookings`) и Kafka.
2. В `appsettings.json` каждого сервиса задайте:

```json
{
  "ConnectionStrings": {
    "AuthDbConnection": "Host=localhost;Port=5432;Database=auth;Username=postgres;Password=postgres",
    "EventsDbConnection": "Host=localhost;Port=5432;Database=events;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "замените-на-длинный-случайный-секрет-32-байта-минимум",
    "Issuer": "TicketNest",
    "Audience": "TicketNest.Api",
    "LifetimeMinutes": 60
  },
  "Kafka": {
    "BaseUrl": "localhost:9092",
    "Login": "",
    "Password": ""
  }
}
```

> Для Auth используется ключ `AuthDbConnection`, для Events и Bookings — `EventsDbConnection` (он указывает на соответствующую БД сервиса).

3. Запустите сервисы (каждый в своём терминале):

```bash
dotnet run --project TicketNest.Auth.Api
dotnet run --project TicketNest.Events.Api
dotnet run --project TicketNest.Bookings.Api
```

Схемы БД создаются/обновляются через EF Core migrations при старте (`app.Services.RunMigrations()`). Топики Kafka создаются при старте автоматически.

4. Swagger UI каждого сервиса:
   - Auth: `http://localhost:5001/swagger` (или `https://localhost:5001/swagger`)
   - Events: `http://localhost:5002/swagger`
   - Bookings: `http://localhost:5003/swagger`

### Авторизация в Swagger

1. Выполните `POST /auth/login` (или `POST /auth/register` с `"role": 1` для админа).
2. Нажмите **Authorize** в Swagger UI и введите `Bearer <token>`.

## Запуск в Docker

Для каждого сервиса подготовлен многоступенчатый `Dockerfile` (`Dockerfile.Auth`, `Dockerfile.Events`, `Dockerfile.Bookings`), а `docker-compose.yml` поднимает всю инфраструктуру:

- 3 экземпляра PostgreSQL (`postgres-auth`, `postgres-events`, `postgres-bookings`);
- Kafka (`confluentinc/cp-kafka` в режиме KRaft, включено авто-создание топиков как запасной вариант);
- 3 сервиса, получающих строки подключения и адрес Kafka через переменные окружения.

```bash
docker compose up --build
```

Сервисы будут доступны на портах `5001` (auth), `5002` (events), `5003` (bookings). Переменные окружения (`ConnectionStrings__*`, `Kafka__*`) переопределяют значения из `appsettings.json`.

## Наблюдаемость (OpenTelemetry, Prometheus, Jaeger, Grafana)

В сервисы добавлен стек наблюдаемости: **OpenTelemetry SDK** собирает трейсы и метрики, **Prometheus** хранит метрики, **Jaeger** — трейсы, **Grafana** визуализирует метрики, а **Serilog** приводит логи к структурированному JSON-формату.

### Что добавлено

| Компонент | Назначение |
|-----------|------------|
| OpenTelemetry SDK | Автоматическая инструментация входящих/исходящих HTTP-запросов и запросов EF Core, сбор метрик ASP.NET Core и рантайма .NET |
| Prometheus | Сбор и хранение метрик (скрейпинг `/metrics` каждого сервиса) |
| Jaeger | Приём и хранение трейсов по OTLP (gRPC) |
| Grafana | Дашборд с latency, throughput и error rate |
| Serilog + `CompactJsonFormatter` | Логи в структурированном JSON |

Пакеты OpenTelemetry и Serilog подключены в Presentation-проекте каждого сервиса (`TicketNest.Auth.Api`, `TicketNest.Events.Api`, `TicketNest.Bookings.Api`).

### Сигналы

- **Трейсы** — `AddAspNetCoreInstrumentation()`, `AddHttpClientInstrumentation()`, `AddEntityFrameworkCoreInstrumentation()`, экспорт в Jaeger через OTLP (`AddOtlpExporter`).
- **Метрики** — `AddAspNetCoreInstrumentation()`, `AddRuntimeInstrumentation()`, экспорт через Prometheus (`AddPrometheusExporter`).
- **Логи** — Serilog с `CompactJsonFormatter`: каждая строка лога — полноценный JSON-объект с уровнем, временной меткой, источником и сообщением.

### Имена сервисов и адрес OTLP

Имя ресурса задаётся в `Startup` каждого сервиса (`.ConfigureResource(r => r.AddService(...))`):

| Сервис | Имя в Jaeger / метках Prometheus |
|--------|----------------------------------|
| `TicketNest.Auth.Api` | `users-service` |
| `TicketNest.Events.Api` | `events-service` |
| `TicketNest.Bookings.Api` | `bookings-service` |

Адрес OTLP задаётся в `appsettings.json` (`Otlp:Endpoint`, по умолчанию `http://localhost:4317`) и переопределяется переменной окружения `Otlp__Endpoint` при запуске в Docker (`http://jaeger:4317`).

### Порты

| Инструмент | URL / порт | Назначение |
|------------|-----------|------------|
| Prometheus | http://localhost:9090 | UI, статус скрейпинга (Status → Targets) |
| Jaeger | http://localhost:16686 | UI, поиск трейсов |
| Jaeger OTLP | localhost:4317 | приём трейсов по gRPC |
| Grafana | http://localhost:3000 | дашборд (логин `admin`, пароль `admin`) |
| Метрики сервисов | `/metrics` | `5001` (auth), `5002` (events), `5003` (bookings) |

### Запуск стека мониторинга

```bash
docker compose up --build
```

Поднимаются все сервисы и инфраструктура, включая контейнеры `prometheus`, `jaeger` и `grafana`. Конфигурация скрейпинга описана в `prometheus.yml`, а datasource и дашборд Grafana подключаются автоматически через provisioning (`grafana/provisioning`, `grafana/dashboards`).

Проверка:

1. **Метрики**: `curl http://localhost:5002/metrics` возвращает данные в формате Prometheus.
2. **Prometheus**: http://localhost:9090/targets — все job'ы (`users-service`, `events-service`, `bookings-service`) в статусе **UP**.
3. **Jaeger**: http://localhost:16686 — трейсы сервисов с HTTP-спанами (`GET Events` и т.п.) и SQL-спанами EF Core.
4. **Grafana**: http://localhost:3000 — дашборд **TicketNest - Service Overview**.

### Дашборд Grafana

Дашборд `grafana/dashboards/ticketnest-overview.json` добавлен в репозиторий и загружается автоматически. Панели построены на метриках ASP.NET Core:

| Панель | Метрика |
|--------|---------|
| HTTP latency (p50 / p95 / p99) | `http_server_request_duration_seconds_bucket` |
| Throughput (RPS) | `http_server_request_duration_seconds_count` |
| Error rate (HTTP 5xx) | `http_server_request_duration_seconds_count{http_response_status_code=~"5.."}` |
| Active requests | `http_server_active_requests` |

Источник данных — Prometheus (`http://prometheus:9090`), задаётся в `grafana/provisioning/datasources/prometheus.yml`.

## Структура проектов

```
TicketNest/
├── TicketNest.Auth.Api / .Application.Auth / .Domain.Auth / .DataAccess.Auth
├── TicketNest.Events.Api / .Application.Events / .Domain.Events / .DataAccess.Events / .Queues.Events
├── TicketNest.Bookings.Api / .Application.Bookings / .Domain.Bookings / .DataAccess.Bookings / .Queues.Bookings
├── TicketNest.Contracts        # общий Kafka-контракт (топики + сообщения)
├── TicketNest.Kafka            # транспорт Kafka + создание топиков
├── TicketNest.Shared           # Result-паттерн, Guard, TokenUser
├── TicketNest.Infrastructure   # JWT (генерация/валидация), хеширование
├── TicketNest.UnitTests / TicketNest.IntegrationTests
├── Dockerfile.Auth / Dockerfile.Events / Dockerfile.Bookings
├── prometheus.yml               # конфигурация скрейпинга метрик
├── grafana/                     # provisioning + JSON дашборда
└── docker-compose.yml
```

## Тесты

```bash
dotnet test TicketNest.UnitTests        # юнит-тесты (NUnit)
dotnet test TicketNest.IntegrationTests  # интеграционные тесты (xUnit + Testcontainers, требует Docker)
```

## Стратегия кеширования (Redis)

Сервис **Events** использует **Redis** для кеширования двух сценариев, снижая нагрузку на PostgreSQL при частых запросах.

### Архитектура кеширования

Абстракция кеша (`ICacheService`) определена в слое **Application** (`TicketNest.Application.Events.Cache`), а реализация на Redis (`RedisCacheService`) — в слое **Infrastructure** (`TicketNest.Infrastructure.Events`). Это позволяет слою Application не зависеть от конкретной библиотеки кеша.

При недоступности Redis ошибки логируются, но не пробрасываются клиенту — сервис деградирует без ошибки, запросы идут напрямую в базу данных. Реализовано через `NoOpCacheService` при отключённом Redis или обработку исключений в `RedisCacheService`.

### Что кешируется и почему

| Сценарий | Ключ кеша | Стратегия | Обоснование |
|----------|-----------|-----------|-------------|
| **Получение события по ID** (`GET /events/{id}`) | `event:{id}` | **Cache-Aside + инвалидация при записи** | Точечный запрос по первичному ключу. Данные меняются при бронировании/отмене, поэтому кеш должен обновляться при изменениях. Инвалидация предпочтительнее обновления, так как уменьшает количество операций записи в кеш. |
| **Топ-10 популярных событий** (`GET /events/top`) | `events:top10` | **Cache-Aside по TTL** | Рейтинговый агрегат, запрашиваемый анонимными пользователями. Небольшое устаревание (TTL) некритично — список меняется нечасто, а явная инвалидация при каждом бронировании была бы избыточной. |

### Выбор TTL

- **Событие по ID**: 60 секунд — баланс между актуальностью данных и нагрузкой на БД. События меняются при бронировании, поэтому слишком длинный TTL приведёт к устаревшим данным.
- **Топ-10**: 30 секунд — рейтинг обновляется при бронировании/отмене, но для пользователей задержка в 30 секунд незаметна.

Значения TTL заданы как константы в `CacheKeys`:

### Порядок операций при изменении данных

Инвалидация кеша выполняется **после** успешной записи в базу данных:

```
1. Запись в БД (SaveChanges)
2. Удаление ключей из кеша (RemoveAsync)
```

Если выполнение оборвётся между шагами 1 и 2, база останется в актуальном состоянии, а кеш при следующем запросе просто обновится. Кеш не будет содержать устаревших данных — только может отдавать их до истечения TTL.

### Кеширование через Kafka-обработчики

Обработчики Kafka-сообщений (`BookingCreatedBackgroundService`, `BookingCancelledBackgroundService`) изменяют данные событий (резервирование/освобождение мест). После успешного сохранения в БД они **инвалидируют кеш** для затронутого события и топа.

### Конфигурация

Параметры кеша вынесены в `appsettings.json`:

```json
{
  "Cache": {
    "ConnectionString": "localhost:6379",
    "IsEnabled": true
  }
}
```

Переменная окружения `Cache__ConnectionString` автоматически переопределяет значение из конфигурации при запуске в Docker.

### Тестирование

Написаны unit-тесты для всех трёх сценариев:
- **Попадание в кеш**: при наличии данных в кеше репозиторий не вызывается
- **Промах кеша**: данные берутся из репозитория и сохраняются в кеш
- **Инвалидация**: при мутирующих операциях (Create, Change, Delete) ключи удаляются из кеша

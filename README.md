# TicketNest API

Бэкенд-сервис для управления событиями и бронированиями (билетная система).

## 🚀 Технологии

- **.NET 9**, **ASP.NET Core**
- **API Versioning** (`Asp.Versioning`)
- **Clean Architecture** (Onion): Api → Application → Domain ← Shared
- **Domain-Driven Design** (Value Objects, Domain Services, Aggregates)
- **Result pattern** для обработки ошибок (`Result<TValue, TError>`)
- **Background Services** для асинхронной обработки
- **NSubstitute**, **NUnit**, **FluentAssertions** (тесты)

## 📦 Функционал

### Events Controller (`/events`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/events` | Список событий (пагинация, фильтрация по названию и датам) |
| GET | `/events/{id}` | Получить событие по GUID |
| POST | `/events` | Создать новое событие |
| PUT | `/events/{id}` | Обновить событие |
| DELETE | `/events/{id}` | Удалить событие |
| POST | `/events/{id}/book` | Создать бронирование на событие (202 Accepted) |

### Booking Controller (`/bookings`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/bookings/{id}` | Получить информацию о бронировании |

### Модель Booking

Бронирование создаётся в статусе `Pending` и асинхронно подтверждается фоновым процессом.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | `Guid` | Уникальный идентификатор бронирования |
| `EventId` | `Guid` | Идентификатор события |
| `Status` | `BookingStatus` | Текущий статус брони |
| `CreatedAt` | `DateTime` | Дата и время создания (UTC) |
| `ProcessedAt` | `DateTime?` | Дата и время обработки (заполняется при подтверждении) |

**Статусы бронирования (`BookingStatus`)**:

| Статус | Значение | Описание |
|--------|----------|----------|
| `Pending` | 0 | Бронь создана, ожидает подтверждения |
| `Confirmed` | 1 | Бронь подтверждена билетной системой |
| `Rejected` | 2 | Бронь отклонена |

### Логика фоновой обработки

После создания бронирования сервис помещает сообщение в очередь (`BookingQueue`). Фоновый сервис `BookingConfirmationBackgroundService` работает в бесконечном цикле:

1. Читает сообщение из очереди
2. Загружает бронь из репозитория
3. Вызывает `BookingConfirmationService.Confirm()`:
   - Эмулирует обращение к внешней билетной системе (10 сек)
   - Переводит бронь в статус `Confirmed`
4. Сохраняет обновлённую бронь
5. Подтверждает (коммитит) сообщение в очереди

---

## 🏗️ Архитектура проекта

```
TicketNest/
├── Api/                              # Презентационный слой
│   ├── Controllers/V1/
│   │   ├── EventsController.cs       # CRUD событий + book
│   │   └── BookingController.cs      # GET бронирований
│   ├── Models/V1/                    # DTO
│   ├── Mappers/                      # Domain → DTO
│   └── Middlewares/                  # Exception Handling
├── Application/                      # Слой приложения
│   ├── Services/
│   │   ├── Events/                   # IEventService / EventService
│   │   └── Bookings/                 # IBookingService / BookingService
│   └── BackgroundServices/           # BookingConfirmationBackgroundService
├── Domain/                           # Доменный слой
│   ├── Models/
│   │   ├── Events/Event.cs
│   │   └── Bookings/Booking.cs       # Booking, BookingStatus
│   ├── Services/Bookings/            # IBookingFactory, IBookingConfirmationService
│   ├── Repositories/                 # IEventsRepository, IBookingRepository
│   └── ValueObjects/                 # EventId, EventTitle, EventDescription
├── DataAccess.Events/                # In-memory реализация репозиториев
└── Shared/                           # Guard, Result Pattern, Helpers
```

## 🛠️ Установка и запуск

```bash
git clone https://github.com/RedPowDan/TicketNest
cd TicketNest
dotnet restore
dotnet run --project TicketNest.Api
```

## 🧪 Тесты

```bash
dotnet test
```

---

## 📋 Пример сценария использования

```http
### 1. Создать событие
POST /events
Content-Type: application/json

{
  "title": "Концерт",
  "description": "Живое выступление",
  "startAt": "2026-06-15T19:00:00Z",
  "endAt": "2026-06-15T23:00:00Z"
}

→ 201 Created
{
  "result": { "id": "evt-001", "title": "Концерт", ... },
  "error": null
}

### 2. Забронировать место на событие
POST /events/evt-001/book

→ 202 Accepted
Location: /bookings/bkg-001
{
  "result": { "id": "bkg-001", "eventId": "evt-001", "status": "Pending" },
  "error": null
}

### 3. Проверить статус брони (сразу после создания)
GET /bookings/bkg-001

→ 200 OK
{
  "result": { "id": "bkg-001", "eventId": "evt-001", "status": "Pending" },
  "error": null
}

### 4. Проверить статус брони (через ~10 сек, после подтверждения)
GET /bookings/bkg-001

→ 200 OK
{
  "result": { "id": "bkg-001", "eventId": "evt-001", "status": "Confirmed" },
  "error": null
}
```


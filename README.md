# TicketNest API

Бэкенд-сервис для управления событиями (билетная система). На текущем этапе реализован базовый CRUD для работы с событиями.

## 🚀 Технологии

- **.NET** (современная версия)
- **ASP.NET Core** с поддержкой versioning (`Asp.Versioning`)
- **Domain-Driven Design** (Value Objects: `EventId`, `EventTitle`, `EventDescription`)
- **Result pattern** для обработки ошибок

## 📦 Текущий функционал

### Events Controller (`/api/v1/events`)

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/events` | Получить список всех событий |
| GET | `/events/{id}` | Получить событие по GUID |
| POST | `/events` | Создать новое событие |
| PUT | `/events/{id}` | Обновить существующее событие |
| DELETE | `/events/{id}` | Удалить событие |

## 🏗️ Архитектура проекта
TicketNest/  
├── Api/ # Презентационный слой  
│ ├── Controllers/V1/ # ✅ EventsController (текущий)  
│ ├── Models/V1/ # DTO (EventRequest, EventResponse)  
│ ├── Mappers/ # Маппинг Domain -> DTO  
│ └── Constants/ # Константы (версионирование)  
├── Application/ # Слой приложения  
│ └── Services/Events/ # Бизнес-логика (IEventService)  
└── Domain/ # Слой домена  
└── ValueObjects/ # EventId, EventTitle, EventDescription  

## 🛠️ Установка и запуск

### Требования
- .NET 8.0 SDK или выше

### Локальный запуск

```bash
# Клонировать репозиторий
git clone <your-repo-url>
cd TicketNest

# Восстановить зависимости
dotnet restore

# Запустить проект
dotnet run --project TicketNest.Api


# TrueCodeTest - Микросервисная архитектура

Тестовое задание на реализацию микросервисной архитектуры с использованием .NET 8, PostgreSQL, Clean Architecture, CQRS и FluentValidation.

## Структура проекта

Решение организовано по следующим папкам:

### Services/
Содержит бизнес-микросервисы с Clean Architecture:
- **UserService/** - Микросервис пользователей (регистрация, логин, логаут)
  - TrueCodeTest.UserService.API
  - TrueCodeTest.UserService.Application
  - TrueCodeTest.UserService.Domain
  - TrueCodeTest.UserService.Infrastructure
- **FinanceService/** - Микросервис финансов (получение курсов валют, управление избранными валютами)
  - TrueCodeTest.FinanceService.API
  - TrueCodeTest.FinanceService.Application
  - TrueCodeTest.FinanceService.Domain
  - TrueCodeTest.FinanceService.Infrastructure

### Infrastructure/
Содержит инфраструктурные сервисы:
- **TrueCodeTest.MigrationService** - Сервис для выполнения миграций базы данных
- **TrueCodeTest.CurrencyBackgroundService** - Фоновый сервис для обновления курсов валют с сайта ЦБ РФ
- **TrueCodeTest.ApiGateway** - API Gateway для маршрутизации запросов к микросервисам

### Shared/
Содержит общие компоненты:
- **TrueCodeTest.Shared.Domain** - Общие сущности, DbContext, Behaviors и Validators

### Tests/
Содержит unit-тесты:
- **TrueCodeTest.UserService.Tests**
- **TrueCodeTest.FinanceService.Tests**

## Архитектура

- **Clean Architecture** - применена для UserService и FinanceService
- **CQRS** - реализован через MediatR для разделения команд и запросов
- **JWT Authentication** - используется для авторизации
- **PostgreSQL** - база данных
- **YARP** - для реализации API Gateway
- **FluentValidation** - для валидации входных данных
- **ResultBase** - базовый класс для унифицированных ответов

## Требования

- .NET 8 SDK
- PostgreSQL
- Docker (опционально, для PostgreSQL)

## Настройка базы данных

### Вариант 1: Docker (рекомендуется)

```bash
docker run -d \
  --name truecode-postgres \
  -e POSTGRES_DB=TrueCodeTestDb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -v truecode-postgres-data:/var/lib/postgresql/data \
  postgres:16
```

### Вариант 2: Локальная PostgreSQL

Убедитесь, что PostgreSQL запущен и создана база `TrueCodeTestDb` с пользователем `postgres`.

## Запуск проекта

### 1. Выполнение миграций

Запустите MigrationService и выполните миграцию через endpoint:

```bash
cd Infrastructure/TrueCodeTest.MigrationService
dotnet run
```

Затем отправьте POST запрос на `http://localhost:5000/migrate` или используйте Swagger UI.

### 2. Запуск фонового сервиса обновления валют

```bash
cd Infrastructure/TrueCodeTest.CurrencyBackgroundService
dotnet run
```

Сервис автоматически обновляет курсы валют каждые 24 часа с сайта ЦБ РФ. Интервал настраивается в `appsettings.json`.

### 3. Запуск микросервисов

В отдельных терминалах запустите:

**UserService:**
```bash
cd Services/UserService/TrueCodeTest.UserService.API
dotnet run
```
Сервис будет доступен на `http://localhost:5001`

**FinanceService:**
```bash
cd Services/FinanceService/TrueCodeTest.FinanceService.API
dotnet run
```
Сервис будет доступен на `http://localhost:5002`

### 4. Запуск API Gateway

```bash
cd Infrastructure/TrueCodeTest.ApiGateway
dotnet run
```
API Gateway будет доступен на `http://localhost:5090`

## Использование API

### Регистрация пользователя

```bash
POST http://localhost:5090/api/user/register
Content-Type: application/json

{
  "name": "testuser",
  "password": "password123"
}
```

**Правила валидации:**
- Имя пользователя: не пустое, максимум 50 символов, только латинские буквы, цифры и подчеркивания
- Пароль: не пустой, от 6 до 100 символов

### Вход в систему

```bash
POST http://localhost:5090/api/user/login
Content-Type: application/json

{
  "name": "testuser",
  "password": "password123"
}
```

Ответ содержит JWT токен, который нужно использовать для последующих запросов.

### Выход из системы

```bash
POST http://localhost:5090/api/user/logout
Authorization: Bearer {your_jwt_token}
```

### Получение избранных валют пользователя

```bash
GET http://localhost:5090/api/finance/currencies
Authorization: Bearer {your_jwt_token}
```

### Добавление валюты в избранное

```bash
POST http://localhost:5090/api/finance/favorites
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "CurrencyName": "Доллар США"
}
```

**Правила валидации:**
- CurrencyName: не пустой, максимум 100 символов
- UserId: должен быть положительным числом

### Удаление валюты из избранного

```bash
DELETE http://localhost:5090/api/finance/favorites
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "CurrencyName": "Доллар США"
}
```

## Запуск тестов

```bash
# Тесты UserService
cd Tests/TrueCodeTest.UserService.Tests
dotnet test

# Тесты FinanceService
cd Tests/TrueCodeTest.FinanceService.Tests
dotnet test

# Или все тесты сразу из корня решения
dotnet test
```

Тесты проверяют:
- Бизнес-логику хендлеров
- Валидацию входных данных
- Сообщения об ошибках
- Сценарии с валидными и невалидными данными

## Структура базы данных

### Таблица `currency`
- `id` - первичный ключ
- `name` - название валюты
- `rate` - курс валюты к рублю

### Таблица `user`
- `id` - первичный ключ
- `name` - имя пользователя
- `password` - хешированный пароль (SHA256)

### Таблица `user_currency`
- `user_id` - внешний ключ на таблицу `user`
- `currency_id` - внешний ключ на таблицу `currency`
- Составной первичный ключ: `(user_id, currency_id)`

## Конфигурация

### JWT аутентификация

JWT настройки находятся в `appsettings.json` каждого сервиса:

```json
"Jwt": {
  "Key": "key",
  "Issuer": "TrueCodeTest",
  "Audience": "TrueCodeTest"
}
```

В production окружении используйте переменные окружения или секреты.

### Фоновый сервис валют

Настройки в `appsettings.json` `CurrencyBackgroundService`:

```json
"CurrencyUpdate": {
  "CbrUrl": "http://www.cbr.ru/scripts/XML_daily.asp",
  "UpdateIntervalHours": 24
}
```

## Технологии

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8.0
- PostgreSQL (Npgsql)
- MediatR (CQRS)
- FluentValidation (валидация)
- JWT Bearer Authentication
- YARP (API Gateway)
- xUnit (тестирование)
- Moq (моки для тестов)

## Особенности реализации

- **JWT токены** хранятся в `appsettings.json`, в production рекомендуется вынести в секреты
- **Общая база данных** для всех сервисов (в production можно разделить)
- **Фоновый сервис** обновляет курсы валют каждые 24 часа, при первом запуске обновление происходит сразу
- **API Gateway** выполняет маршрутизацию запросов и централизованную аутентификацию
- **FluentValidation** используется для валидации входных данных с кастомными сообщениями об ошибках
- **ValidationBehavior** автоматически применяет валидацию в MediatR pipeline
- **BaseValidator** предоставляет общие правила валидации для уменьшения дублирования кода
- **ResultBase** унифицирует ответы всех хендлеров
- **Тесты** используют InMemory базу данных и покрывают основные сценарии команд и запросов, включая проверку сообщений валидации

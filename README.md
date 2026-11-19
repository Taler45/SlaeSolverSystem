# SlaeSolverSystem

Користувач реєструється, створює задачу (вказує розмір системи або вводить свою матрицю) і бачить, як вона рахується. Запити приходять на балансувальник, а він розкидає їх між двома серверами. Задачі стають у спільну чергу в базі даних, і рахує їх той сервер, у якого є вільне місце. Матриці та результати зберігаються в S3-сховищі (SeaweedFS).

## Що всередині

- `loadbalancer/` - балансувальник навантаження, також віддає фронтенд
- `backend/` - сервер (API + фоновий обробник задач), запускається у двох екземплярах
- `frontend/` - сторінки на HTML/JS
- PostgreSQL - користувачі та задачі
- SeaweedFS - файли з матрицями і результатами

Використано: .NET 10, ASP.NET Core, EF Core, PostgreSQL, SeaweedFS.

## Як запустити

### Що потрібно встановити

- .NET 10 SDK
- PostgreSQL
- SeaweedFS (`weed.exe` з релізів на GitHub), я поклав його в `C:\seaweed`
- `dotnet tool install --global dotnet-ef`
- `dotnet dev-certs https --trust`

### Файли .env

Вони не лежать у репозиторії, їх треба створити самому.

`backend/.env`:

```
ConnectionStrings__SlaeSolverContext=Host=localhost;Port=5432;Database=slae;Username=postgres;Password=...
Jwt__SecretKey=...
Server__InternalApiKey=...
AWS_ACCESS_KEY_ID=...
AWS_SECRET_ACCESS_KEY=...
```

`loadbalancer/.env`:

```
Jwt__SecretKey=...                # такий самий, як у backend
LoadBalancer__InternalApiKey=...  # такий самий, як Server__InternalApiKey
```

### SeaweedFS

Створити `C:\seaweed\s3.json`:

```json
{
  "identities": [
    {
      "name": "slae-server",
      "credentials": [ { "accessKey": "...", "secretKey": "..." } ],
      "actions": ["Read", "Write", "List"]
    }
  ]
}
```

Ключі тут мають бути ті самі, що `AWS_ACCESS_KEY_ID` і `AWS_SECRET_ACCESS_KEY` у `backend/.env`.

Запустити:

```
cd C:\seaweed
.\weed.exe server -dir="C:\seaweed-data" -s3 -s3.config="C:\seaweed\s3.json"
```

Перший раз треба створити бакет:

```
.\weed.exe shell
s3.bucket.create -name slae-storage
```

### База даних

```
cd backend
dotnet ef database update --project SlaeSolver.DataAccess --startup-project SlaeSolver.API
```

### Запуск

Кожне в окремому терміналі:

```
cd backend\SlaeSolver.API
dotnet run --launch-profile srv1

cd backend\SlaeSolver.API
dotnet run --launch-profile srv2

cd loadbalancer\LoadBalancer.API
dotnet run --launch-profile https
```

Сайт відкривається на https://localhost:7127

Щоб зайти в адмінку, треба зареєструватися і в базі змінити собі роль:

```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'твій email';
```

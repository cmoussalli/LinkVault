# LinkVault

A self-hosted link management portal built with **.NET 9** and **Blazor Server**. Create short links that redirect to any URL, or serve static content (HTML, JSON, images, text) directly from a short code — and track every click.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)
![Blazor](https://img.shields.io/badge/Blazor-Server-5C2D91)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **Short links** — map a short code (e.g. `/aB3xY9`) to any destination URL with a 302 redirect.
- **Content links** — serve a file from `wwwroot/content` under a short code instead of redirecting. Supports HTML, JSON, JPEG, PNG, GIF and plain text.
- **Auto-generated codes** — 6-character alphanumeric codes, checked for uniqueness; or supply your own custom code.
- **Expiry and activation** — set an optional expiry date, or toggle a link inactive to take it offline without deleting it.
- **Click tracking** — every hit logs IP address (honouring `X-Forwarded-For` / `X-Real-IP`), user agent, referrer and timestamp.
- **Analytics dashboard** — totals for today / this week / this month, top-performing links, recent activity, and per-link breakdowns by date, country and referrer.
- **Admin portal** — searchable link list, create/edit forms with validation, and a Bootstrap 5 UI.
- **Authentication** — login backed by [CMouss.IdentityFramework](https://www.nuget.org/packages/CMouss.IdentityFramework) with its Blazor UI components.

## Tech stack

| Layer | Technology |
|---|---|
| Framework | .NET 9 / ASP.NET Core |
| UI | Blazor Server (`InteractiveServer` render mode), Bootstrap 5, Bootstrap Icons |
| Data | Entity Framework Core 9 (SQL Server and SQLite providers) |
| Identity | CMouss.IdentityFramework + CMouss.IdentityFramework.BlazorUI |
| Redirects | ASP.NET Core MVC controller (`RedirectController`) |

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (or switch to SQLite — see [Configuration](#configuration))

### Run it

```bash
git clone https://github.com/cmoussalli/LinkVault.git
cd LinkVault
dotnet run --project LinkVault.Portal
```

The app starts on <http://localhost:5111>. The database schema and identity master data are created automatically on first launch (`EnsureCreated` + `InsertMasterData`).

Sign in at `/login` with the administrator account you configured (see [Configuration](#configuration)).

## Configuration

Identity and database settings are read from configuration under the `Identity` section. `appsettings.json` holds the non-secret shape:

```json
{
  "Identity": {
    "DatabaseType": "MSSQL",
    "ConnectionString": "",
    "TokenEncryptionKey": "",
    "AdministratorUserName": "admin",
    "AdministratorPassword": "",
    "AdministratorRoleName": "Administrators",
    "DefaultListPageSize": 25,
    "TokenLifeTimeDays": 365,
    "AllowUserMultipleSessions": false
  }
}
```

Secrets are **not** committed. Supply them locally with [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
cd LinkVault.Portal
dotnet user-secrets set "Identity:ConnectionString" "Server=…;Database=LinkVault;User Id=…;Password=…;TrustServerCertificate=True"
dotnet user-secrets set "Identity:TokenEncryptionKey" "<a long random key>"
dotnet user-secrets set "Identity:AdministratorPassword" "<a strong password>"
```

In production, use environment variables instead — `Identity__ConnectionString`, `Identity__TokenEncryptionKey`, `Identity__AdministratorPassword`. The app throws a descriptive exception at startup if any of these three are missing.

To use SQLite instead of SQL Server, set `Identity:DatabaseType` to `SQLite` and point the connection string at a file, e.g. `Data Source=LinkVault.db`.

## Routes

| Route | Purpose |
|---|---|
| `/` | Public landing page |
| `/login` | Administrator sign-in |
| `/admin` | Dashboard — totals, top links, recent activity |
| `/admin/links` | Link list with search |
| `/admin/links/create` | Create a link |
| `/admin/links/edit/{id}` | Edit a link |
| `/admin/analytics` | Analytics overview |
| `/admin/analytics/{shortCode}` | Per-link analytics |
| `/{shortCode}` | Catch-all — resolves a short code to a redirect or content response |

Because `/{shortCode}` is a catch-all, short codes must not collide with the reserved paths above.

## Project structure

```
LinkVault.Portal/
├── Components/
│   ├── Layout/          MainLayout, AdminLayout, LoginLayout, NavMenu
│   └── Pages/
│       ├── Account/     Login
│       ├── Admin/       Dashboard, Links, CreateEditLink, Analytics
│       └── Home.razor   Public landing page
├── Controllers/
│   └── RedirectController.cs   Short-code resolution, click logging, content serving
├── Data/
│   └── LinkVaultDbContext.cs   Extends IDFDBContext; Links + ClickLogs
├── Models/
│   ├── Link.cs          ShortCode, RedirectUrl, Type, ContentPath, ExpiresAt, ClickCount
│   └── ClickLog.cs      IpAddress, UserAgent, Referrer, ClickedAt, Country, City
├── Services/
│   ├── LinkService.cs        CRUD + short-code generation
│   ├── AnalyticsService.cs   Click logging and aggregate queries
│   └── ContentService.cs     Content file listing, upload and validation
└── wwwroot/content/     Files served by content links
```

## How a click is handled

1. `RedirectController` receives `GET /{shortCode}`.
2. The code is looked up; missing, inactive or expired links return **404**.
3. Client IP, user agent and referrer are captured and written to `ClickLogs`, and the link's `ClickCount` is incremented.
4. A `Redirect` link returns a redirect to its destination; a `Content` link streams the matching file from `wwwroot/content` with the appropriate content type.

## Known limitations

- Country and city on click logs are stored as `"Unknown"` — no IP geolocation provider is wired up yet.
- Services are instantiated directly rather than through dependency injection, and each creates its own `DbContext`.
- Admin routes are not yet protected by an authorization attribute; the login page exists but does not gate `/admin/*`.

## License

[MIT](LICENSE) © 2025 Caesar Moussalli

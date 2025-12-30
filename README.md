# D8Auth 🔐

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/) [![C#](https://img.shields.io/badge/C%23-12.0-green)](https://learn.microsoft.com/en-us/dotnet/csharp/) [![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-purple)](https://dotnet.microsoft.com/apps/aspnet) [![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-orange)](https://docs.microsoft.com/ef/) [![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE.txt)

## Introduction

Welcome to the D8Auth repository! This project demonstrates the use of **Identity API endpoints** introduced in .NET 8. It provides a modern, minimal API approach to authentication and authorization using ASP.NET Core Identity with Entity Framework Core and SQL Server.

## Technology Stack

- .NET 8.0
- C# 12.0
- ASP.NET Core Identity
- Entity Framework Core 8.0
- SQL Server
- Swagger/OpenAPI

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) [![Download .NET 8](https://img.shields.io/badge/Download-.NET%208-blue)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or VS Code

## Project Structure

```
D8Auth/
├── Controllers/          # API Controllers
├── Data/                
│   ├── DataContext.cs    # EF Core DbContext
│   └── ApplicationUser.cs # Custom user model
├── Migrations/           # EF Core migrations
├── Program.cs            # Application entry point and configuration
└── appsettings.json      # Configuration settings
```

## Future Plans

This project is a learning exercise, and I plan to extend it with:

- ✅ Custom user properties
- 🔄 Two-Factor Authentication (2FA) implementation
- 📧 Email confirmation and password reset flows
- 🎨 Custom identity UI
- 🔐 OAuth/OpenID Connect integration
- 🛡️ Role-based authorization
- 📊 User activity logging and auditing

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to discuss what you would like to change.

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for more details.

## Resources

- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Identity API Endpoints (.NET 8)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

## About

A .NET 8 learning project exploring Identity API endpoints with custom user properties and ASP.NET Core Identity.

---

Made with ❤️ by [zeecorleone](https://github.com/zeecorleone)

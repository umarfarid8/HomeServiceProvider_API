# Home Service Provider (HSP) — ASP.NET Core Web API Core

This repository hosts the core operational business engine, data layers, and API endpoint routing structures for the Home Service Provider platform. It acts as a secure, fast data processor handling authentication, relational persistence execution, and programmatic intelligence features.

## 🛠️ Technology Stack & Architecture
* **Framework:** C# ASP.NET Core Web API (Minimal APIs & Controller Layouts)
* **Data Access Engine:** Entity Framework Core (Code-First migration workflow)
* **Database Target:** Microsoft SQL Server
* **Authentication Framework:** JWT Bearer tokens + Google OAuth 2.0 Identity tokens validation
* **Document Engine:** QuestPDF for programmatic PDF invoice rendering

## 🧠 Architectural Highlights
* **Hybrid Semantic Matching Engine:** Avoids slow, expensive coordinate math inside external LLM prompt contexts. Uses an isolated service wrapper (`IAiMatchingService`) to classify text user search intent down to a single category label, then uses database LINQ optimizations via EF Core to filter records spatially within targeted city limits.
* **Secure Data Transfer Patterns:** Decouples core database tracking entities from open network vectors by mapping all outbound payloads through strongly typed Data Transfer Objects (DTOs).
* **Robust Exception Interception:** Wrapped in global filter scopes to gracefully handle network dropouts or database sequence issues, preventing unexpected pipeline system failures.

## 🚀 Local Deployment Setup

1. **Prerequisites:**
   Ensure you have the .NET SDK (v8.0+), SQL Server, and Visual Studio installed locally.

2. **Database Connection Configuration:**
   Open the `appsettings.json` file in the root API project folder and update your local connection string parameters and external AI secret configurations:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_LOCAL_SQL_SERVER;Database=HomeServiceProviderDb;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "AI": {
       "ApiKey": "your-llm-api-token-key-here"
     }
   }
Initialize the Database Schema Layout:
Open the Package Manager Console inside Visual Studio and run the following command to apply database structural code migrations:

PowerShell
Update-Database
Launch the Core Server:
Press F5 or click Run inside Visual Studio. The API will initialize its security middlewares, apply CORS rules whitelisting your frontend execution endpoints, and launch your operational Swagger documentation explorer.


---


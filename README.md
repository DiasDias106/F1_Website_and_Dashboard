<h1 align="center">🏎️ F1 Website & Dashboard</h1>

<p align="center">
  <i>A Formula 1-themed ASP.NET MVC platform: a public fan website with minigames, plus a "Paddock Club" dashboard to manage teams, drivers, tracks and the race calendar.</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/ASP.NET%20MVC-5-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET MVC 5"/>
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#"/>
  <img src="https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server"/>
  <img src="https://img.shields.io/badge/Entity%20Framework-6-1a1b27?style=for-the-badge" alt="EF6"/>
  <img src="https://img.shields.io/badge/Chart.js-FF6384?style=for-the-badge&logo=chartdotjs&logoColor=white" alt="Chart.js"/>
  <img src="https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap"/>
</p>

---

## 📖 About

**F1 Website & Dashboard** is a full-stack web application built with **ASP.NET MVC 5** on **.NET Framework 4.8**. It has two sides:

- a **public website** for fans, with F1 content, team pages with 3D car models, and a collection of mini-apps;
- a **management dashboard** where a logged-in "team principal" manages all F1 data, views charts, and exports reports.

## ✨ Features

### 🏁 Public Website
- 📄 Pages about the F1 universe, teams and races
- 🚗 **Team pages with interactive 3D car models** *(models by third parties)*
- 🎮 **Mini-apps**: 💱 currency converter (Open Exchange Rates API), 🖼️ image converter, 🔐 text encryptor, 🔊 text-to-speech, and more
- 🌲 F1-themed **TreeView** navigation
- 📬 **Newsletter** subscription and contact email

### 📊 Paddock Club Dashboard
- 🔐 **Authentication & roles** with ASP.NET Identity (register, login, role management, profile editing, account deletion)
- 🏎️ **Teams, drivers & car components**: full CRUD
- 📅 **Race calendar & tracks**: full CRUD with filtering and **pagination**
- 📤 **Export to PDF and Excel** (Rotativa / iTextSharp and ClosedXML)
- 📈 **Chart.js dashboards** with custom visualisations
- 🧩 **LINQ-based** querying and statistics
- 🛒 **F1 store** with cart and admin management
- ✉️ **Mass and custom emails** sent from the dashboard

## 🏗️ Architecture

```
F1_website/
├── Controllers/        # Home, F1, dashboard, Account, Manage, Pesquisas
├── Models/             # EF6 entities, Identity models, Email & Newsletter services
├── Views/              # Razor views (Home, F1, dashboard, Account, Manage, Shared)
├── App_Data/           # LocalDB database (F1DBSite.mdf), SQL scripts, JSON data
├── App_Start/          # Routing, bundling, Identity and OWIN auth configuration
├── javascript/         # Front-end scripts (currency converter, games, tools…)
├── Content/            # CSS (Bootstrap, Font Awesome, custom styles)
└── Web.config          # Connection strings and app configuration
```

## 🚀 Getting Started

### Requirements
- Windows with **Visual Studio 2019/2022** (*ASP.NET and web development* workload)
- **.NET Framework 4.8**
- **SQL Server Express LocalDB**

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/DiasDias106/F1_Website_and_Dashboard.git
   ```
2. The repository contains the application source but **not** the `.csproj` / `.sln` files. In Visual Studio, create an **ASP.NET Web Application (.NET Framework 4.8), MVC** project named `F1_website`, then add the contents of the `F1_website/` folder to it.
3. Restore the NuGet packages listed in `packages.config`.
4. The database is included in `App_Data/F1DBSite.mdf` and attaches automatically via LocalDB (see `Web.config`). SQL scripts for the schema are also provided (`tabelas_site.sql`, `site_queries.sql`, `paginas_queries.sql`).
5. *(Optional)* Configure the integrations:
   - **Email:** set the sender address and Gmail **App Password** in `Models/Email.cs` and `Models/Newsletter.cs`
   - **Currency converter:** add your [Open Exchange Rates](https://openexchangerates.org/) key in `javascript/conversor_de_moedas.js`
6. Press **F5** to run.

> 🔒 Never commit real credentials or API keys.

## 🛠️ Built With

| Area | Technology |
|------|------------|
| Framework | ASP.NET MVC 5, .NET Framework 4.8, OWIN |
| Data | SQL Server LocalDB, Entity Framework 6, LINQ |
| Auth | ASP.NET Identity |
| Reports | Rotativa, iTextSharp (PDF) · ClosedXML (Excel) |
| Front end | Razor, Bootstrap, jQuery, Chart.js, Font Awesome |
| Other | PagedList, Newtonsoft.Json |

## ⚠️ Disclaimer

Fan-made educational project. *Formula 1*, team names, logos and 3D models belong to their respective owners.

## 📜 License

Distributed under the terms of the [LICENSE](LICENSE) file in this repository.

---

<p align="center">Made by <a href="https://github.com/DiasDias106">Francisco Dias</a></p>

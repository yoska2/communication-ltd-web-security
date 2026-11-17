# Communication LTD

**Secure vs. Vulnerable Web Application**  
*C# · ASP.NET Core MVC · .NET 5 · MySQL · Web Security*

## Overview

Communication LTD is a web application for a fictional communications company. It supports user accounts, customer records, and password management.

The project includes **two versions of the same application**. The goal is to compare insecure coding patterns with safer implementation techniques, especially for **SQL Injection** and **Stored Cross-Site Scripting (XSS)**.

## Two Versions

| Area | `LTD_Vulnerable` | `LTD_Secure` |
|---|---|---|
| Purpose | Demonstrates intentionally unsafe coding patterns | Demonstrates selected safer coding techniques |
| SQL queries | Some queries build SQL by joining user input into strings | Reviewed queries use SQL parameters to separate values from commands |
| Customer name input | Does not apply the reviewed HTML-encoding step | Applies HTML encoding to the customer full name before saving |
| Customer table | Uses `Html.Raw` to display customer names | Also uses `Html.Raw` to display customer names |

## Main Features

- User registration and login
- Session-based access to account and customer pages
- Customer creation and customer lists
- Password changes with password-policy checks
- Password history checks against recent passwords
- Password reset codes sent by email
- Reset-code expiration checks

## Technology Stack

| Layer | Technologies |
|---|---|
| Backend | C#, .NET 5, ASP.NET Core MVC |
| Views | Razor (`.cshtml`) |
| Database | MySQL, SQL, `MySql.Data` 9.5.0 |
| Frontend | HTML, CSS, JavaScript, Bootstrap, jQuery |
| Form validation | jQuery Validation, jQuery Validation Unobtrusive |
| Session management | ASP.NET Core Session |
| Email | SMTP through Mailjet (`System.Net.Mail.SmtpClient`) |

## Security Mechanisms in the Code

### Password storage and verification

The `PasswordHasher` class uses **PBKDF2 with SHA-256**, a **random 16-byte salt**, **100,000 iterations**, and a **32-byte derived hash**. Password verification uses `CryptographicOperations.FixedTimeEquals`.

### Password management

The application includes configurable password rules, password history checks, password-change workflows, and password-reset codes with an expiration check. A separate SHA-1 helper is used to generate reset codes; **SHA-1 is not the password-storage algorithm**.

### Sessions and transport

The code uses ASP.NET Core Session to track signed-in users. Session cookies are configured with `HttpOnly`, and the middleware includes HTTPS redirection and HSTS outside development mode.

### SQL Injection and Stored XSS

The most important comparison is between SQL queries built with string concatenation and SQL queries that use parameters. For customer names, the secure version applies HTML encoding during input handling, while the vulnerable version does not apply that step. Both versions render the stored name with `Html.Raw`, which is an important limitation to understand.

## Repository Structure

```text
Secure-Web-Application/
├── src/
│   ├── LTD_Secure/
│   └── LTD_Vulnerable/
├── docs/
│   ├── Project_Summary.pdf
│   ├── Project_Summary_HE.pdf
│   └── Technical_Documentation.pdf
├── README.md
└── .gitignore
```

Each application version contains its own controllers, models, data-access classes, security helpers, services, Razor views, and static web files.

## Running the Project Locally

**Requirements:** A development environment capable of building .NET 5 applications, a MySQL database, and appropriate local application settings. .NET 5 is an older, unsupported framework, so this project is provided as a historical educational implementation.

1. Open either `src/LTD_Secure/LTD_Secure.sln` or `src/LTD_Vulnerable/LTD_Vulnerable.sln` in Visual Studio.
2. Configure the MySQL connection in that version's database configuration for your own local environment.
3. Create or provide the database tables expected by the application. A ready-to-import database schema is **not documented as part of this repository**.
4. To test password-reset email, configure valid SMTP credentials for the email service.
5. Restore the project's NuGet packages, build, and run the selected version.

The two versions should be run and evaluated separately. **Do not expose the intentionally vulnerable version to the public internet.**

## Documentation

- [`Project Summary`](docs/Project_Summary.pdf) — one-page overview of the project, features, and two-version design.
- [`Project Summary`](docs/Project_Summary_HE.pdf) — project summary in Hebrew.
- [`Technical Documentation`](docs/Technical_Documentation.pdf) — detailed explanation of the architecture, dependencies, account workflows, database access, security mechanisms, and code-based comparison.

## Project Summary

This repository demonstrates how implementation choices can affect application security. Its main value is the **side-by-side comparison of two C# web applications**, supported by practical features such as password hashing, session management, database operations, and email-based password recovery.

# Lahi Task Management System

A full-stack, enterprise-grade Task Management application built with **.NET 8** and **Angular 18**.

---

## Table of Contents
- [📁 Project Structure](#-project-structure)
- [🏛 High-Level Architecture](#-high-level-architecture)
  - [Backend (Clean Architecture)](#backend-clean-architecture)
  - [Frontend (Modern Angular)](#frontend-modern-angular)
- [🚀 Quick Start (Docker)](#-quick-start-docker)
- [💻 Local Development Setup](#-local-development-setup)
  - [Backend (.NET 8)](#backend-net-8)
  - [Frontend (Angular 18)](#frontend-angular-18)
- [🛠 Features & Business Rules](#-features--business-rules)
  - [1. Authentication](#1-authentication)
  - [2. Task Management](#2-task-management)
  - [3. Reporting & Dashboard](#3-reporting--dashboard)
  - [4. Notifications](#4-notifications)
- [📊 Database Schema](#-database-schema)
- [🔒 Security Measures](#-security-measures)
- [🧪 Testing](#-testing)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [📞 Contact](#-contact)

---

## 📁 Project Structure

The project is organized into a monorepo-like structure, housing both the .NET backend and Angular frontend, along with supporting files for development and deployment.

```
.
├── .angular/                  # Angular CLI configuration files
├── .git/                      # Git version control metadata
├── .idea/                     # IntelliJ/Android Studio project files
├── .ai/                       # AI-related configuration or generated files (if applicable)
├── src/                       # Primary source code for both backend and frontend
│   ├── API/                   # .NET Web API project (Backend - API Layer)
│   ├── Application/           # .NET project for business logic (Backend - Application Layer)
│   ├── Domain/                # .NET project for core entities and rules (Backend - Domain Layer)
│   ├── Infrastructure/        # .NET project for data persistence and external services (Backend - Infrastructure Layer)
│   ├── app/                   # Main Angular application components and modules (Frontend)
│   ├── environments/          # Angular environment-specific configurations
│   └── ...                    # Other Angular root files (main.ts, index.html, styles.scss)
├── tests/                     # Contains additional test suites (e.g., end-to-end tests, if any)
├── angular.json               # Angular CLI workspace configuration
├── package.json               # Frontend (Node.js/npm) dependencies and scripts
├── package-lock.json          # Records the exact versions of frontend dependencies
├── tsconfig.json              # Base TypeScript configuration for the project
├── tsconfig.app.json          # TypeScript configuration specific to the Angular application
├── TaskManagement.sln         # Visual Studio solution file for the .NET backend projects
├── database_setup.sql         # SQL script for initial database schema setup
├── Dockerfile                 # Dockerfile for building the main application image
├── Dockerfile.test            # Dockerfile for building a test environment image
├── docker-compose.yml         # Docker Compose configuration for multi-service deployment
├── docker-compose.override.yml# Docker Compose override for local development settings
└── README.md                  # Project README file
```

### Key Directories Explained:

*   **`src/`**: The heart of the application, containing all actively developed source code.
    *   **Backend (.NET 8 - Clean Architecture)**: Organized into `API`, `Application`, `Domain`, and `Infrastructure` projects, reflecting the Clean Architecture principles.
    *   **Frontend (Angular 18)**: The `app/` directory holds the main Angular application, with `environments/` for configuration.
*   **`database_setup.sql`**: Provides the necessary SQL commands to set up the application's database schema.
*   **`docker-compose.yml`**: Defines the multi-container Docker application, including the API, database, and mail server.
*   **`TaskManagement.sln`**: The primary solution file for opening and managing the .NET backend projects in Visual Studio.
*   **`tests/`**: A dedicated directory for any additional test suites that are not part of the .NET solution's project-specific tests.

---

## 🏛 High-Level Architecture

### Backend (Clean Architecture)
The backend follows Uncle Bob's Clean Architecture principles, ensuring separation of concerns and high testability.

- **API Layer**: ASP.NET Core 8 Web API. Handles HTTP requests, JWT authentication, and global error handling.
- **Application Layer**: Contains business logic, DTOs, interfaces, and FluentValidation rules.
- **Domain Layer**: Pure C# logic containing core Entities and Enums. No external dependencies.
- **Infrastructure Layer**: Implementation of persistence (EF Core), Identity (JWT), and external services (Email, File Storage).

### Frontend (Modern Angular)
- **Framework**: Angular 18 (Standalone Components).
- **State Management**: **Angular Signals** for reactive, high-performance UI state.
- **UI System**: Angular Material (Material 3) with a custom responsive theme.
- **Communication**: Reactive forms with custom validators and an RxJS-based API layer.

---

## 🚀 Quick Start (Docker)

The fastest way to run the entire stack (API + DB + Mail) is using Docker Compose.

```bash
# 1. Start all backend services
docker-compose up -d

# 2. Wait for the "Database is ready" message in logs
docker logs -f taskmanagement-api

# 3. Install frontend dependencies
npm install

# 4. Start the frontend
npm start
```

- **Frontend**: http://localhost:4200
- **API (Swagger)**: http://localhost:8081/swagger
- **Emails (MailHog)**: http://localhost:8025

---

## 💻 Local Development Setup

### Backend (.NET 8)

1.  **Prerequisites**:
    *   .NET 8 SDK installed.
    *   SQL Server (or Docker for SQL Server instance).
    *   SMTP server (e.g., MailHog via Docker, or a local SMTP service).

2.  **Database Setup**:
    *   Ensure your SQL Server is running.
    *   Update the connection string in `src/API/appsettings.Development.json` to point to your local SQL Server instance.
    *   Run Entity Framework Core migrations to create the database schema:
        ```bash
        cd src/Infrastructure
        dotnet ef database update --project ../API
        ```

3.  **Run the API**:
    ```bash
    cd src/API
    dotnet run
    ```
    The API will typically run on `http://localhost:5000` or `http://localhost:5001` (HTTPS).

### Frontend (Angular 18)

1.  **Prerequisites**:
    *   Node.js (LTS version recommended) and npm installed.
    *   Angular CLI installed globally (`npm install -g @angular/cli`).

2.  **Install Dependencies**:
    ```bash
    cd src/Frontend
    npm install
    ```

3.  **Configure API Endpoint**:
    *   If your backend is not running on `http://localhost:5000`, update the API base URL in `src/Frontend/src/environments/environment.development.ts`.

4.  **Run the Frontend**:
    ```bash
    cd src/Frontend
    ng serve
    ```
    The frontend will be accessible at `http://localhost:4200`.

---

## 🛠 Features & Business Rules

### 1. Authentication
- JWT-based auth with **Refresh Token rotation**.
- "Remember Me" support (extended token lifetime).
- Numeric Role mapping (Admin=1, Employee=2) for cross-platform compatibility.

### 2. Task Management
- **Security**: Employees only see tasks assigned to them; Admins see all.
- **Business Rule**: Completed tasks cannot be edited by employees (locked in UI and API).
- **Validation**: Due Date must be $\ge$ Start Date.
- **Attachments**: Support for PDF/JPG/PNG up to 5MB with sanitized file storage.

### 3. Reporting & Dashboard
- Role-based Dashboard stats.
- **Admin Reports**: Completed Tasks, Pending Tasks, and Employee-wise performance.
- **Exports**: Direct download to **Excel (.xlsx)** and **CSV**.

### 4. Notifications
- **Task Assigned**: Real-time notification + Email.
- **Due Soon**: Background job runs hourly to alert users of tasks due within 24 hours.
- **Task Completed**: Alerts the creator when a task is finished.

---

## 📊 Database Schema
The database is structured for performance with appropriate indexing on query-heavy columns (`Email`, `AssignedToId`, `Status`).

- **SQL Script**: The complete T-SQL schema can be found in `database_setup.sql` in the root folder of this repository.

---

## 🔒 Security Measures
- **Password Hashing**: BCrypt (Work Factor 11).
- **CORS**: Restricted to the frontend origin.
- **Rate Limiting**: Protection against brute-force login attempts.
- **Path Traversal Protection**: Sanitized file uploads and restricted directory access.
- **Soft Delete**: Global EF Core query filters ensure data integrity.

---

## 🧪 Testing
The solution includes three test suites:
1.  **API Integration Tests**: End-to-end flow validation using `WebApplicationFactory`.
2.  **Application Unit Tests**: Logic validation for Services and Validators.
3.  **Infrastructure Tests**: Repository verification using EF Core InMemory.

```bash
# Run all tests
dotnet test
```

---

## 🤝 Contributing
We welcome contributions to the Lahi Task Management System! To contribute:

1.  **Fork** the repository.
2.  **Clone** your forked repository to your local machine.
3.  Create a new **branch** for your feature or bug fix (`git checkout -b feature/your-feature-name`).
4.  Make your changes, ensuring they adhere to the project's coding standards.
5.  Write **unit and integration tests** for your changes.
6.  Ensure all existing tests pass (`dotnet test`).
7.  **Commit** your changes with a clear and descriptive message.
8.  **Push** your branch to your forked repository.
9.  Open a **Pull Request** to the `main` branch of the original repository, describing your changes and their benefits.

---

## 📄 License
This project is licensed under the [MIT License](LICENSE). See the `LICENSE` file in the root of the repository for full details.

---

## 📞 Contact
For any questions, issues, or support, please open an issue on the GitHub repository or contact [jay@example.com](mailto:jay@example.com).

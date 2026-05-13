# Auren
Auren is a full-stack personal finance tracker application built using **.NET Core Web API** and **Angular**.  
# 📂 Project Structure

```bash
Auren/
|
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
|   |   |   |   ├── auth/
|   |   |   |   ├── layout/
|   |   |   |   ├── models/
|   |   |   |   ├── routes/
|   |   |   |   ├── services/
│   │   │   ├── features/
|   |   |   |   ├── categories/
|   |   |   |   ├── dashboard/
|   |   |   |   ├── profile/
|   |   |   |   ├── transactions/
│   │   │   ├── shared/
│
├── backend/
│   ├── Auren.API/
│   ├── Auren.Application/
│   ├── Auren.Domain/
│   ├── Auren.Infrastructure/
```

---
# 🚀 Getting Started

## Prerequisites

Ensure you have the following installed:

- Node.js
- Angular CLI
- .NET SDK 8+
- SQL Server
- Visual Studio / VS Code

---
# ⚙️ Installation

## Clone the repository

```bash
git clone https://github.com/Belleza411/Auren-Personal-Finance-Tracker.git
```

---

## Backend Setup

Navigate to backend directory:

```bash
cd backend
```

Restore dependencies:

```bash
dotnet restore
```

Apply migrations:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

---

## Frontend Setup

Navigate to frontend directory:

```bash
cd frontend
```

Install dependencies:

```bash
npm install
```

Run Angular development server:

```bash
ng serve
```

# 🤝 Contributing

Contributions, suggestions, and improvements are welcome.

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Open a pull request

---

# 📄 License

This project is licensed under the MIT License.

---

# 👨‍💻 Author

Developed by **Aevan Belleza**

Auren — Personal Finance Tracker

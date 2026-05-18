# BH Telecom Izvještaji

## Setup

### Frontend
```bash
cd frontend
npm install
```

### Backend
```bash
cd backend
dotnet restore
dotnet run
```

Backend radi na `http://localhost:5006`.

## CORS
CORS je konfigurisan u `backend/Program.cs` sa `AllowAnyOrigin()` za development.

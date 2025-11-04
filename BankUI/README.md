# BankUI

A React + TypeScript frontend application connected to the MyApp .NET backend.

## Setup

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

The frontend will run on `http://localhost:3000`

## Backend Connection

This application is configured to connect to the MyApp backend running on:
- **HTTPS**: `https://localhost:7059`
- **HTTP**: `http://localhost:5250`

### Before Running

Make sure the MyApp backend is running. Navigate to the backend directory and run:
```bash
cd ../new/MyApp/MyApp
dotnet run
```

The backend should be accessible at `https://localhost:7059` with Swagger UI at `https://localhost:7059/swagger`

## Environment Variables

You can optionally create a `.env` file to customize the API base URL:

```
VITE_API_BASE_URL=https://localhost:7059
```

## Project Structure

```
src/
├── config/
│   └── api.config.ts      # API configuration
├── services/
│   └── api.service.ts      # Base API service (axios)
├── App.tsx                 # Main App component
├── main.tsx                # Entry point
└── index.css               # Global styles
```

## API Service

The API service is set up in `src/services/api.service.ts` and uses axios for HTTP requests. It includes:
- Request/response interceptors
- Error handling
- TypeScript support
- Configurable base URL

## CORS Configuration

The MyApp backend has been configured with CORS to allow requests from:
- `http://localhost:3000` (Vite default)
- `http://localhost:5173` (Vite alternate)
- `http://localhost:5174` (Vite alternate)

## Development

- Frontend: `npm run dev` (runs on port 3000)
- Build: `npm run build`
- Preview: `npm run preview`


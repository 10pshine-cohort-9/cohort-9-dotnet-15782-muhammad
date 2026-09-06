# cohort-9-dotnet-15782-muhammad
Cohort 9 — .NET Fullstack (.NET+ReactJS) assignment for Muhammad Hassan Adil

## Running Locally

### Backend
1. Requires SQL Server (Express or full) running locally
2. Set User Secrets in `TaskManagementTool.Api`:
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>"
   dotnet user-secrets set "Jwt:Key" "<any random string, 32+ characters>"
3. Run the API (F5 in Visual Studio, or `dotnet run`) — database and seed data are created automatically on first run (Development mode only)

### Frontend
1. Copy `.env.example` to `.env`
2. Update `VITE_API_BASE_URL` to match the port shown when the backend starts
3. `npm install` then `npm run dev`

### Test accounts
Admin and one test user are seeded automatically on first run.
See `test-users-admin.md` for credentials.

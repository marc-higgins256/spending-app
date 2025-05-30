# Spending App

This is a full-stack project with:
- React frontend (Vite) in `frontend/`
- C# ASP.NET Core backend in `backend/`
- MySQL database

## Getting Started (from scratch)

### 1. Clone the repository
```sh
git clone https://github.com/marc-higgins256/spending-app.git
cd spending-app
```

### 2. Set up the Database
- Install MySQL (if not already installed)
- Create a database (e.g. `spending_app`)
- Create a MySQL user and grant privileges
- Update `backend/appsettings.Development.json` with your connection string:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=spending_app;user=YOURUSER;password=YOURPASSWORD;"
  }
  ```
- Apply migrations:
  ```sh
  dotnet ef database update --project backend/backend.csproj --startup-project backend/backend.csproj
  ```

### 3. Set up Mailtrap for Email Testing
- Sign up at [mailtrap.io](https://mailtrap.io/) and create an inbox
- Copy SMTP credentials and add to `backend/appsettings.Development.json`:
  ```json
  "Mailtrap": {
    "Host": "sandbox.smtp.mailtrap.io",
    "Port": 2525,
    "User": "YOUR_MAILTRAP_USERNAME",
    "Pass": "YOUR_MAILTRAP_PASSWORD",
    "From": "no-reply@yourapp.com" // This can be anything, the others, not so much. 
  }
  ```

### 4. Backend Setup
```sh
cd backend
# Restore dependencies
 dotnet restore
# Run the backend
 dotnet run
```

### 5. Frontend Setup
```sh
cd ../frontend
npm install
npm run dev
```
- The app will be available at [http://localhost:5173](http://localhost:5173)

### 6. Running Tests
```sh
cd ../backend.Tests
# Run all backend tests
 dotnet test
```

### 7. Using the App
- Register a new user (any email, Mailtrap will catch it)
- Check your Mailtrap inbox for the confirmation email
- Click the confirmation link/paste the url to confirm your email
- Log in and access protected pages

## Using Project Scripts

Scripts are located in the `scripts/` directory at the root of the repository. These help automate common development tasks:

### Start MySQL Service (requires elevated permissions)
Start the MySQL service (default: MySQL80) on Windows:
```powershell
./scripts/start-mysql.ps1
```

### Stop MySQL Service (requires elevated permissions)
Stop the MySQL service:
```powershell
./scripts/stop-mysql.ps1
```

### Start Backend and Frontend Together
Start both backend and frontend in separate PowerShell windows:
```powershell
./scripts/start-all.ps1
```

- Make sure to run these scripts from a PowerShell terminal. Some scripts may require administrative privileges (e.g., starting/stopping MySQL).

---

For more details, see the subfolder READMEs or code comments(If I've put proper comments).

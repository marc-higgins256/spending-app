# Spending App

This is a full-stack project with:
- React frontend (Vite) in `frontend/`
- C# ASP.NET Core backend in `backend/`
- MySQL database

## Getting Started (from scratch)

### 1. Clone the repository
```sh
git clone <your-repo-url>
cd spending-app
```

### 2. Set up the Database
- Install MySQL (if not already installed)
- Create a database (e.g., `spending_app`)
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
- Click the confirmation link to confirm your email
- Log in and access protected pages

---

For more details, see the subfolder READMEs or code comments.

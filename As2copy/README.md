# FaceSmash API

A .NET 8 Web API application that allows users to upload photos to Azure Blob Storage, manage profiles, and send messages. The application uses SQLite for data persistence and includes a web interface for testing.

## Features

- **User Profile Management**: Create, read, update, and delete user profiles
- **Photo Upload**: Upload photos to Azure Blob Storage with automatic URL generation
- **Messaging System**: Send and retrieve messages between users
- **SQLite Database**: Local data persistence with Entity Framework Core
- **Web Interface**: HTML frontend for testing all API endpoints

## Prerequisites

- .NET 8 SDK
- Azure Storage Account (for photo uploads)
- Visual Studio Code or Visual Studio

## Setup

1. **Clone and Navigate to Project**
   ```bash
   cd /Users/kazumashikata/Desktop/As2copy
   ```

2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

3. **Update Azure Configuration**
   - Open `appsettings.json`
   - Update the `AzureBlobStorage` connection string with your Azure Storage account details
   - Update the `ContainerName` if needed

4. **Create Database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the Web Interface**
   - Open your browser and go to `https://localhost:7000` or `http://localhost:5000`
   - The web interface will be available for testing all API endpoints

## API Endpoints

### Profile Management
- `GET /api/profile` - Get all users
- `GET /api/profile/{id}` - Get user by ID
- `POST /api/profile` - Create new user
- `PUT /api/profile/{id}` - Update user
- `DELETE /api/profile/{id}` - Delete user
- `PUT /api/profile/{id}/rating` - Update user rating

### Photo Upload
- `POST /api/upload/photo?userId={id}` - Upload photo for user
- `GET /api/upload/photos/{userId}` - Get user's photos

### Messaging
- `POST /api/message/send` - Send a message
- `GET /api/message/between/{user1Id}/{user2Id}` - Get conversation between two users
- `DELETE /api/message/{id}` - Delete a message

## Database Schema

### Users Table
- `Id` (Primary Key)
- `Name` (string)
- `Email` (string)
- `PasswordHash` (string)
- `Gender` (string)
- `PhotoUrl` (string)
- `Rating` (int)
- `CreatedAt` (DateTime)

### Messages Table
- `Id` (Primary Key)
- `SenderId` (Foreign Key to Users)
- `ReceiverId` (Foreign Key to Users)
- `Content` (string)
- `SentAt` (DateTime)

## Configuration

The application uses the following configuration in `appsettings.json`:

```json
{
  "AzureBlobStorage": {
    "ConnectionString": "Your Azure Storage Connection String",
    "ContainerName": "useruploads"
  }
}
```

## Testing

Use the included web interface at `https://localhost:7000` to test all API endpoints. The interface provides:

- User management forms
- Photo upload functionality
- Message sending and retrieval
- Real-time API response display

## Project Structure

```
As2copy/
├── FacesmashAPI.csproj    # Project file with dependencies
├── program                # Main application entry point
├── appsettings.json       # Configuration settings
├── AppDbContext           # Entity Framework database context
├── User.cs               # User model
├── Massage               # Message model
├── profileContoroller    # Profile management controller
├── imageUpploadContoroller # Photo upload controller
├── massageContoroller    # Messaging controller
└── wwwroot/
    └── index.html        # Web testing interface
```

## Security Notes

- The current implementation includes hardcoded Azure credentials for demonstration purposes
- In production, use Azure Key Vault or environment variables for sensitive configuration
- Implement proper authentication and authorization
- Add input validation and sanitization
- Use HTTPS in production

## Troubleshooting

1. **Database Issues**: Ensure SQLite database file permissions are correct
2. **Azure Upload Issues**: Verify Azure Storage account credentials and container permissions
3. **CORS Issues**: Check that the CORS policy allows your frontend domain
4. **Port Issues**: Default ports are 5000 (HTTP) and 7000 (HTTPS), update if needed

## Next Steps

- Add user authentication and authorization
- Implement photo compression and resizing
- Add real-time messaging with SignalR
- Create mobile app integration
- Add photo gallery and management features
- Implement user rating and matching system

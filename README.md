# Meeting Room Booking API

This Azure Function allows you to book meeting rooms in Outlook using Microsoft Graph API with application-level authentication.

## Prerequisites

1. .NET 7.0 or later
2. Azure Functions Core Tools
3. Azure CLI (optional, for deployment)
4. An Azure AD application with the following API permissions:
   - `Calendars.ReadWrite` (Application permission)
   - `User.Read.All` (Application permission, for user lookups)

## Setup

1. Clone this repository
2. Update `appsettings.json` with your Azure AD application details:
   ```json
   {
     "ClientId": "your-client-id",
     "ClientSecret": "your-client-secret",
     "TenantId": "your-tenant-id",
     "SystemUserEmail": "system-user@yourdomain.com",
     "RoomEmail": "room@yourdomain.com"
   }
   ```
3. Set these values as environment variables in your Azure Function App settings when deploying to Azure.

## Local Development

1. Install dependencies:
   ```bash
   dotnet restore
   ```

2. Run the function locally:
   ```bash
   func start
   ```

## API Endpoint

### Book a Room

**Endpoint:** `POST /api/BookRoom`

**Request Body:**
```json
{
  "subject": "Team Sync",
  "content": "Team meeting to discuss project updates",
  "start": "2025-08-01T10:00:00",
  "end": "2025-08-01T11:00:00",
  "timeZone": "W. Europe Standard Time",
  "roomEmail": "room301@yourcompany.com",
  "attendees": [
    {
      "email": "user1@yourcompany.com",
      "type": "required"
    },
    {
      "email": "user2@yourcompany.com",
      "type": "optional"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "eventId": "AAMkAG...",
  "webLink": "https://outlook.office365.com/...",
  "message": "Meeting room booked successfully"
}
```

## Azure AD App Registration

1. Go to Azure Portal > Azure Active Directory > App registrations
2. Create a new registration
3. Add a client secret
4. Add the required API permissions
5. Grant admin consent for the permissions
6. Note down the Application (client) ID, Directory (tenant) ID, and client secret

## Deployment

### Azure CLI
```bash
az login
az functionapp deployment source config-zip \
    --resource-group <resource-group> \
    --name <app-name> \
    --src <zip-file-path>
```

### Visual Studio
1. Right-click the project
2. Select "Publish"
3. Follow the wizard to create or select an existing Azure Function App

## Logging

Logs are written to Application Insights if configured. You can view them in the Azure Portal under your Function App's Monitoring section.

## Error Handling

The API returns appropriate HTTP status codes and error messages for different scenarios:
- 400 Bad Request: Invalid input data
- 401 Unauthorized: Authentication failed
- 500 Internal Server Error: Server-side error

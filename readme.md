# Demo REST and Browser Applications

This demo project consists of two applications:
1. Application A (REST Server) - A .NET 9 Minimal API serving versioned REST endpoints with API key authentication
2. Application B (Web Server) - A .NET 9 web application that discovers and consumes Application A's API

## Architecture

The following diagram illustrates the distributed architecture of the demo project:

![Architecture Diagram](architecture-svg.svg)

## Features

### API Versioning
- URL-based versioning (e.g., /api/v1/hello)
- Version headers in responses (X-API-Version, X-API-Supported-Versions)
- Version validation middleware
- Current supported version: 1.0

### Authentication
- API key-based authentication
- Origin validation per API key
- Rate limiting capabilities
- No authentication required for health checks

### Service Discovery
- Automatic registration of Application A instances
- Health monitoring and status tracking
- Support for multiple Application A instances
- Automatic cleanup of inactive instances

## Prerequisites

- Windows 10 or later
- .NET 9 SDK
- Docker Desktop (only to generate the SSL certificates)
- Visual Studio 2022 or VS Code
- cURL (optional, for API testing)

## Project Structure

```
local-rest-browser-demo/
+-- certificates/           # SSL certificates (generated)
+-- ApplicationA-REST/     # REST Server application
+-- ApplicationB-Web/      # Web application
+-- generate-certs.bat     # Certificate generation script
+-- check-install-dotnet.bat # Script to verify if .NET 9 is installed and install it in case is needed
+-- start-application-a-REST.bat # Script to start Application A on port 5001
+-- start-application-a-REST-5003.bat # Script to start Application A on port 5003
+-- start-application-a-REST-5005.bat # Script to start Application A on port 5005
+-- start-application-b-WEB.bat # Script to start Application B
+-- clean-application-a.bat # Script to clean-up Application A build
+-- clean-application-b.bat # Script to clean-up Application B build
+-- README.md
+-- architecture-svg.svg    # Architecture diagram
```

## Setup Instructions

1. Clone or download this repository
2. Open a Command Prompt as Administrator
3. Navigate to the project root folder:
   ```batch
   cd path/to/local-rest-browser-demo
   ```
4. Generate SSL certificates (this script requires Docker to be running locally):
   ```batch
   generate-certs.bat
   ```
   Note: You'll need to accept the security prompts to install the root certificate.

5. Configure API keys in Application A's appsettings.json:
   ```json
   {
     "ApiKeys": [
       {
         "Key": "demo-key-1",
         "Name": "Application B Demo Key",
         "AllowedOrigins": ["https://localhost:5002"],
         "RateLimit": 100,
         "IsEnabled": true
       }
     ]
   }
   ```

## Running the Applications

1. Start Application A (REST Server):
   - Open a new Command Prompt
   - Navigate to the project folder
   - Run one or more instances:
     ```batch
     start-application-a-REST.bat
     start-application-a-REST-5003.bat
     start-application-a-REST-5005.bat
     ```
   - The servers will start at https://localhost:5001, 5003, and 5005 respectively

2. Start Application B (Web Server):
   - Open another Command Prompt
   - Navigate to the project folder
   - Run:
     ```batch
     start-application-b-WEB.bat
     ```
   - The web application will start at https://localhost:5002

## Accessing the Applications

### Application A (REST Server)
- REST API Base URL: https://localhost:5001/api/v1
- All endpoints require API key authentication (X-API-Key header)
- Health check endpoint: GET /api/v1/health (no authentication required)

Available Endpoints:
- GET /api/v1/hello - Returns a greeting message
- POST /api/v1/echo - Echoes back the sent message

### Application B (Web Application)
- Open your web browser
- Navigate to https://localhost:5002
- The interface will show:
  - Available Application A instances with health status
  - API test interface for each instance
  - Automatic health status updates every 30 seconds

## Testing Application A (REST Server) with cURL

Note: The `-k` flag is used to skip SSL certificate verification since we're using self-signed certificates.

1. Test the Health endpoint:
```bash
curl -k https://localhost:5001/api/v1/health
```

2. Test the Hello endpoint:
```bash
curl -k https://localhost:5001/api/v1/hello \
  -H "X-API-Key: demo-key-1"
```

3. Test the Echo endpoint:
```bash
curl -k -X POST https://localhost:5001/api/v1/echo \
  -H "X-API-Key: demo-key-1" \
  -H "Content-Type: application/json" \
  -d "{\"message\":\"Test message\"}"
```

4. Test CORS headers:
```bash
curl -k -X OPTIONS https://localhost:5001/api/v1/hello \
  -H "Origin: https://localhost:5002" \
  -H "Access-Control-Request-Method: GET" \
  -v
```

## Troubleshooting

1. Certificate Issues:
   - Make sure you ran `generate-certs.bat` as Administrator
   - Check that both certificates exist in the `certificates` folder
   - Verify the root certificate is installed in Windows certificate store

2. Port Conflicts:
   - If ports 5001, 5003, 5005, or 5002 are in use, you'll need to modify the ports in:
     - ApplicationA-REST/src/ApplicationA/Program.cs
     - ApplicationB-Web/src/ApplicationB/Program.cs
     - ApplicationB-Web/src/ApplicationB/wwwroot/index.html

3. CORS Issues:
   - If you see CORS errors in the browser console, verify:
     - Application A's CORS policy allows https://localhost:5002
     - You're accessing Application B from the correct URL
     - API key configuration includes the correct allowed origins

4. API Key Issues:
   - Verify the API key is configured in Application A's appsettings.json
   - Check that the X-API-Key header is being sent
   - Verify the origin is allowed for the API key

5. Common Errors:
   - "System.IO.FileNotFoundException: Certificate file not found"
     - Run generate-certs.bat again
   - "System.Net.Http.HttpRequestException: The SSL connection could not be established"
     - Check if the root certificate is properly installed
   - "Invalid API key" or "Unauthorized"
     - Check API key configuration and request headers
   - "Unsupported API version"
     - Verify the endpoint URL includes the correct version (v1)

## Development

### Visual Studio 2022
1. Open each .sln file separately
2. Each project can be run directly from Visual Studio
3. Use IIS Express or Kestrel profile as needed

### VS Code
1. Open each project folder separately
2. Install the C# Dev Kit extension
3. Use the built-in debugger to run the applications

## Security Notes

- The SSL certificates are self-signed and for development only
- Do not use these certificates in production
- The demo uses basic CORS settings for simplicity
- API keys should be properly secured in production environments
- Rate limiting should be configured based on production requirements
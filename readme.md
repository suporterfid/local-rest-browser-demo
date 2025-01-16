# Demo REST and Browser Applications

This demo project consists of two applications:
1. Application A (REST Server) - A .NET 9 Minimal API serving REST endpoints and JavaScript client
2. Application B (Web Server) - A .NET 9 web application that consumes Application A's API

## Prerequisites

- Windows 10 or later
- .NET 9 SDK
- Docker Desktop (required for certificate generation)
- Visual Studio 2022 or VS Code
- cURL (optional, for API testing)

## Project Structure

```
local-rest-browser-demo/
├── certificates/           # SSL certificates (generated)
├── ApplicationA-REST/     # REST Server application
│   └── src/
│       └── ApplicationA/
├── ApplicationB-Web/      # Web application
│   └── src/
│       └── ApplicationB/
├── check-install-dotnet.bat   # .NET SDK installation checker
├── clean-application-a.bat    # Clean script for Application A
├── clean-application-b.bat    # Clean script for Application B
├── generate-certs.bat         # Certificate generation script
├── start-application-a.bat    # Script to start Application A
├── start-application-b.bat    # Script to start Application B
└── README.md
```

## Setup Instructions

1. Clone or download this repository

2. Check .NET Installation:
   ```batch
   check-install-dotnet.bat
   ```
   This script will:
   - Check for .NET 9 SDK installation
   - Download and install .NET 9 SDK if not present
   - Verify the installation

3. Generate SSL Certificates:
   ```batch
   generate-certs.bat
   ```
   Requirements:
   - Must be run as Administrator
   - Docker Desktop must be running
   - Will create and install development certificates
   - Certificates will be stored in the `certificates` folder

## Running the Applications

1. Start Application A (REST Server):
   ```batch
   start-application-a-REST.bat
   ```
   - Starts the REST server at https://localhost:5001
   - Configurable parameters:
     - Host (default: localhost)
     - Port (default: 5001)
     - AllowedOrigins (default: https://localhost:5002)

2. Start Application B (Web Server):
   ```batch
   start-application-b-WEB.bat
   ```
   - Starts the web server at https://localhost:5002
   - Configurable parameters:
     - AppAHost (default: https://localhost:5001)

## Application Features

### Application A (REST Server)
- REST API Base URL: https://localhost:5001/api
- JavaScript Client URL: https://localhost:5001/api-client.js
- Static file serving enabled
- CORS configuration for Application B
- SSL/TLS encryption using custom certificates

Available Endpoints:
- GET /api/hello
  - Returns: `{"message":"Hello from Application A!"}`
  - Logs origin of request
- POST /api/echo
  - Accepts: JSON body
  - Returns: Echo of received message
  - Logs origin of request

### Application B (Web Application)
- Web Interface URL: https://localhost:5002
- Features:
  - Dynamic configuration injection
  - Static file serving
  - SSL/TLS encryption using custom certificates
  - Automatic API client configuration

## Development Tools

### Cleaning Build Artifacts
Two scripts are provided for cleaning build outputs:
```batch
clean-application-a.bat  # Cleans Application A
clean-application-b.bat  # Cleans Application B
```
These scripts:
- Remove Debug/Release builds
- Clean bin and obj directories
- Restore project to pre-build state

### Testing Application A (REST Server) with cURL

Note: Use `-k` flag to skip SSL certificate verification for development certificates.

1. Test Hello Endpoint:
```bash
curl -k https://localhost:5001/api/hello
```

2. Test Echo Endpoint:
```bash
curl -k -X POST https://localhost:5001/api/echo \
  -H "Content-Type: application/json" \
  -d "{\"message\":\"Test message\"}"
```

3. Test CORS Configuration:
```bash
curl -k -X OPTIONS https://localhost:5001/api/hello \
  -H "Origin: https://localhost:5002" \
  -H "Access-Control-Request-Method: GET" \
  -v
```

## Troubleshooting

### Certificate Issues
1. Verify Docker is Running:
   - Docker Desktop must be running for certificate generation
   - Check Docker status: `docker info`

2. Certificate Generation:
   - Run as Administrator
   - Check certificates folder exists
   - Verify root certificate installation in Windows store

3. Certificate Paths:
   - Application A: `certificates/ApplicationA.pfx`
   - Application B: `certificates/ApplicationB.pfx`
   - Root CA: `certificates/DemoRootCA.crt`

### Port Conflicts
Default ports:
- Application A: 5001
- Application B: 5002

To change ports:
1. Application A: Modify `--Port` parameter in start-application-a-REST.bat
2. Application B: Update Program.cs and corresponding ApplicationB configurations

### CORS Issues
Verify:
1. Application A's CORS policy matches Application B's origin
2. AllowedOrigins parameter in start-application-a-REST.bat
3. Browser is using correct URLs (https://localhost:5002)

### Common Errors
1. "Certificate file not found":
   - Run generate-certs.bat again
   - Check certificate paths

2. "SSL connection could not be established":
   - Verify root certificate installation
   - Check certificate password (default: demo123!)

3. ".NET SDK is not installed":
   - Run check-install-dotnet.bat
   - Verify PATH environment variable

## Development Environments

### Visual Studio 2022
1. Open solution files:
   - ApplicationA-REST/ApplicationA.sln
   - ApplicationB-Web/ApplicationB.sln
2. Configure HTTPS certificates in project properties
3. Set environment variables in Debug profile

### VS Code
1. Install extensions:
   - C# Dev Kit
   - .NET Runtime Install Tool
2. Open workspace for each application
3. Configure launch.json for HTTPS

## Security Notes

- All certificates are self-signed for development
- Certificate password is "demo123!" (stored in scripts)
- CORS is configured for specific origins only
- Do not use these certificates in production
- Certificates expire after 365 days
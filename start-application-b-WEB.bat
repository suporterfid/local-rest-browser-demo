REM Path: .\start-application-b-WEB.bat
REM Generated: 2025-01-16 12:02:43
REM Project: Demo REST and Browser Applications

@echo off
setlocal enabledelayedexpansion

echo Starting Application B - Web Server...

REM Check if the directory exists
if not exist ApplicationB-Web\src\ApplicationB (
    echo Error: Application B directory not found.
    echo Please ensure you're running this script from the demo-project root folder.
    pause
    exit /b 1
)

REM Check if certificates exist
if not exist certificates\ApplicationB.pfx (
    echo Error: SSL certificate not found.
    echo Please run generate-certs.bat first to create the required certificates.
    pause
    exit /b 1
)

REM Change to application directory
cd ApplicationB-Web\src\ApplicationB

REM Check if project file exists
if not exist ApplicationB.csproj (
    echo Error: Project file not found.
    echo Please ensure the application is properly set up.
    cd ..\..\..
    pause
    exit /b 1
)

echo Starting Web Server at https://localhost:5002
dotnet run
cd ..\..\..
pause


REM Path: .\start-application-a-REST.bat
REM Generated: 2025-01-16 12:02:43
REM Project: Demo REST and Browser Applications

@echo off
setlocal enabledelayedexpansion

echo Starting Application A - REST Server...

REM Check if the directory exists
if not exist ApplicationA-REST\src\ApplicationA (
    echo Error: Application A directory not found.
    echo Please ensure you're running this script from the demo-project root folder.
    pause
    exit /b 1
)

REM Check if certificates exist
if not exist certificates\ApplicationA.pfx (
    echo Error: SSL certificate not found.
    echo Please run generate-certs.bat first to create the required certificates.
    pause
    exit /b 1
)

REM Change to application directory
cd ApplicationA-REST\src\ApplicationA

REM Check if project file exists
if not exist ApplicationA.csproj (
    echo Error: Project file not found.
    echo Please ensure the application is properly set up.
    cd ..\..\..
    pause
    exit /b 1
)

echo Starting REST Server at https://localhost:5001
dotnet run
cd ..\..\..
pause


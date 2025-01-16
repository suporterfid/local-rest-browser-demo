REM Path: .\clean-application-a.bat
REM Generated: 2025-01-16 12:02:43
REM Project: Demo REST and Browser Applications

@echo off
setlocal enabledelayedexpansion

echo Cleaning Application A build artifacts...

REM Check if the directory exists
if not exist ApplicationA-REST\src\ApplicationA (
    echo Error: Application A directory not found.
    echo Please ensure you're running this script from the demo-project root folder.
    pause
    exit /b 1
)

cd ApplicationA-REST\src\ApplicationA

REM Clean the build artifacts
echo Cleaning build artifacts...
dotnet clean -c Debug
dotnet clean -c Release

REM Remove additional build directories
if exist bin (
    echo Removing bin directory...
    rd /s /q bin
)

if exist obj (
    echo Removing obj directory...
    rd /s /q obj
)

cd ..\..\..

echo Application A cleanup completed successfully!
pause

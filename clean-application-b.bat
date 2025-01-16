REM Path: .\clean-application-b.bat
REM Generated: 2025-01-16 12:02:43
REM Project: Demo REST and Browser Applications

@echo off
setlocal enabledelayedexpansion

echo Cleaning Application B build artifacts...

REM Check if the directory exists
if not exist ApplicationB-Web\src\ApplicationB (
    echo Error: Application B directory not found.
    echo Please ensure you're running this script from the demo-project root folder.
    pause
    exit /b 1
)

cd ApplicationB-Web\src\ApplicationB

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

echo Application B cleanup completed successfully!
pause

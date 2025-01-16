@echo off
setlocal enabledelayedexpansion

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo This script requires administrator privileges.
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo Checking for .NET 9 SDK installation...

REM Check if dotnet CLI is available
where dotnet >nul 2>&1
if %errorLevel% neq 0 (
    echo .NET SDK is not installed.
    goto :install_dotnet
)

REM Check installed .NET versions
dotnet --list-sdks > sdks.txt
findstr /C:"9." sdks.txt >nul
if %errorLevel% neq 0 (
    echo .NET 9 SDK is not installed.
    goto :install_dotnet
) else (
    echo .NET 9 SDK is already installed.
    del sdks.txt
    goto :end
)

:install_dotnet
echo Downloading .NET 9 SDK installer...
del sdks.txt 2>nul

REM Create a temporary directory for the download
mkdir dotnet_temp 2>nul
cd dotnet_temp

REM Download the installer
curl -L -o dotnet-installer.exe https://download.visualstudio.microsoft.com/download/pr/5f46239c-783c-4d49-a4a2-cd5b0a47ec51/9b72af54efd90a3874b63e4dd43855e7/dotnet-sdk-9.0.102-win-x64.exe

if %errorLevel% neq 0 (
    echo Failed to download .NET 9 SDK installer.
    echo Please visit https://dotnet.microsoft.com/download/dotnet/9.0 to download and install manually.
    cd ..
    rmdir /s /q dotnet_temp
    pause
    exit /b 1
)

echo Installing .NET 9 SDK...
dotnet-installer.exe /install /quiet /norestart

if %errorLevel% neq 0 (
    echo Failed to install .NET 9 SDK.
    echo Please visit https://dotnet.microsoft.com/download/dotnet/9.0 to download and install manually.
    cd ..
    rmdir /s /q dotnet_temp
    pause
    exit /b 1
)

echo Cleaning up...
cd ..
rmdir /s /q dotnet_temp

echo .NET 9 SDK installation completed successfully.

:verify_installation
echo Verifying installation...
dotnet --version
if %errorLevel% neq 0 (
    echo Failed to verify .NET installation.
    echo Please try restarting your computer and running the script again.
    pause
    exit /b 1
)

:end
echo Setup completed successfully!
echo You may need to restart your terminal or computer for the changes to take effect.
pause
exit /b 0
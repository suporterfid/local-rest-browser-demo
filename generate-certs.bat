REM Path: .\generate-certs.bat
REM Generated: 2025-01-16 12:02:43
REM Project: Demo REST and Browser Applications
REM 
@echo off
setlocal enabledelayedexpansion

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo This script requires administrator privileges.
    echo Please right-click on the script and select "Run as administrator"
    pause
    exit /b 1
)

REM Check if Docker is running
docker info >nul 2>&1
if %errorLevel% neq 0 (
    echo Docker is not running. Please start Docker Desktop and try again.
    pause
    exit /b 1
)

REM Check if certificates directory exists and remove it if it does
if exist certificates (
    rmdir /s /q certificates
)
mkdir certificates

REM Create OpenSSL configuration
mkdir certificates\config
echo [req] > certificates\config\openssl.cnf
echo default_bits = 2048 >> certificates\config\openssl.cnf
echo prompt = no >> certificates\config\openssl.cnf
echo default_md = sha256 >> certificates\config\openssl.cnf
echo distinguished_name = dn >> certificates\config\openssl.cnf
echo [dn] >> certificates\config\openssl.cnf
echo C = US >> certificates\config\openssl.cnf
echo ST = State >> certificates\config\openssl.cnf
echo L = City >> certificates\config\openssl.cnf
echo O = Demo Organization >> certificates\config\openssl.cnf
echo OU = Development >> certificates\config\openssl.cnf
echo CN = Demo Root CA >> certificates\config\openssl.cnf
echo [v3_ca] >> certificates\config\openssl.cnf
echo subjectKeyIdentifier=hash >> certificates\config\openssl.cnf
echo authorityKeyIdentifier=keyid:always,issuer >> certificates\config\openssl.cnf
echo basicConstraints = critical,CA:true >> certificates\config\openssl.cnf
echo keyUsage = critical, digitalSignature, cRLSign, keyCertSign >> certificates\config\openssl.cnf

echo Generating certificates using Docker...

REM Generate Root CA
docker run --rm -v "%cd%\certificates:/certs" -v "%cd%\certificates\config:/config" alpine/openssl genrsa -out /certs/DemoRootCA.key 2048
if %errorLevel% neq 0 goto :error

docker run --rm -v "%cd%\certificates:/certs" -v "%cd%\certificates\config:/config" alpine/openssl req -x509 -new -nodes -key /certs/DemoRootCA.key -sha256 -days 365 -out /certs/DemoRootCA.crt -config /config/openssl.cnf -extensions v3_ca
if %errorLevel% neq 0 goto :error

REM Generate ApplicationA certificate
docker run --rm -v "%cd%\certificates:/certs" alpine/openssl genrsa -out /certs/ApplicationA.key 2048
if %errorLevel% neq 0 goto :error

docker run --rm -v "%cd%\certificates:/certs" alpine/openssl req -new -key /certs/ApplicationA.key -out /certs/ApplicationA.csr -subj "/C=US/ST=State/L=City/O=Demo Organization/OU=Development/CN=localhost"
if %errorLevel% neq 0 goto :error

docker run --rm -v "%cd%\certificates:/certs" alpine/openssl x509 -req -in /certs/ApplicationA.csr -CA /certs/DemoRootCA.crt -CAkey /certs/DemoRootCA.key -CAcreateserial -out /certs/ApplicationA.crt -days 365 -sha256
if %errorLevel% neq 0 goto :error

docker run --rm -v "%cd%\certificates:/certs" alpine/openssl pkcs12 -export -out /certs/ApplicationA.pfx -inkey /certs/ApplicationA.key -in /certs/ApplicationA.crt -certfile /certs/DemoRootCA.crt -passout pass:demo123!
if %errorLevel% neq 0 goto :error

REM Generate ApplicationB certificate
docker run --rm -v "%cd%\certificates:/certs" alpine/openssl genrsa -out /certs/ApplicationB.key 2048
if %errorLevel% neq 0 goto :error

docker run --rm -v "%cd%\certificates:/certs" alpine/openssl req -new -key /certs/ApplicationB.key -out /certs/ApplicationB.csr -subj "/C=US/ST=State/L=City/O=Demo Organization/OU=Development/CN=localhost"
if %errorLevel% neq 0 goto :error

docker run --rm -v "%cd%\certificates:/certs" alpine/openssl x509 -req -in /certs/ApplicationB.csr -CA /certs/DemoRootCA.crt -CAkey /certs/DemoRootCA.key -CAcreateserial -out /certs/ApplicationB.crt -days 365 -sha256
if %errorLevel% neq 0 goto :error

docker run --rm -v "%cd%\certificates:/certs" alpine/openssl pkcs12 -export -out /certs/ApplicationB.pfx -inkey /certs/ApplicationB.key -in /certs/ApplicationB.crt -certfile /certs/DemoRootCA.crt -passout pass:demo123!
if %errorLevel% neq 0 goto :error

REM Clean up temporary files
if exist certificates\*.csr del /f /q certificates\*.csr
if exist certificates\*.key del /f /q certificates\*.key
if exist certificates\*.srl del /f /q certificates\*.srl
if exist certificates\config rmdir /s /q certificates\config

echo Installing root certificate...
certutil -addstore -f "Root" certificates\DemoRootCA.crt
if %errorLevel% neq 0 (
    echo Failed to install root certificate. Please check if you're running as administrator.
    goto :error
)

echo Certificate generation completed successfully!
goto :end

:error
echo An error occurred during certificate generation.
exit /b 1

:end
pause

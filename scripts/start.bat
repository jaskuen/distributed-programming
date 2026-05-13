@echo off
set "ROOT=%~dp0.."

cd /d "%ROOT%"
start "DockerInstance" docker compose up

start "ValuatorInstance1" /D "%ROOT%\Valuator" dotnet run --urls=http://0.0.0.0:5001
start "ValuatorInstance2" /D "%ROOT%\Valuator" dotnet run --urls=http://0.0.0.0:5002

start "RankInstance1" /D "%ROOT%\RankCalculator" dotnet run --urls=http://localhost:5003
start "RankInstance2" /D "%ROOT%\RankCalculator" dotnet run --urls=http://localhost:5004

start "EventsLoggerInstance" /D "%ROOT%\EventsLogger" dotnet run --urls=http://localhost:5005

if exist "C:\nginx\nginx.exe" (
    copy /Y "%ROOT%\nginx\conf\nginx.conf" "C:\nginx\conf\nginx.conf"
    cd /d C:\nginx
    nginx -s stop
    start "NginxInstance" nginx
) else (
    echo C:\nginx\nginx.exe not found
)

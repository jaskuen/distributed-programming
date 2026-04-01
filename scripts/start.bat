cd /d ../.
start "DockerInstance" docker compose up

cd /d ..\Valuator

@echo off
start "ValuatorInstance1" dotnet run --urls=http://localhost:5001
start "ValuatorInstance2" dotnet run --urls=http://localhost:5002

cd /d ..\RankCalculator

@echo off
start "RankInstance1" dotnet run --urls=http://localhost:5003
start "RankInstance2" dotnet run --urls=http://localhost:5004

cd /d ..\EventsLogger

@echo off
start "EventsLoggerInstance" dotnet run --urls=http://localhost:5005


cd /d C:\nginx
stop nginx
start nginx
nginx -s reload

cd /d ..\..\
start "DockerInstance" docker compose up

cd /d .\Valuator

@echo off
start "Instance1" dotnet run --urls=http://localhost:5001
start "Instance2" dotnet run --urls=http://localhost:5002

cd /d C:\nginx
stop nginx
start nginx
nginx -s reload

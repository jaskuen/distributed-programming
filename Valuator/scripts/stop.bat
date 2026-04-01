@echo off
taskkill /FI "WINDOWTITLE eq Instance1" /T /F
taskkill /FI "WINDOWTITLE eq Instance2" /T /F
taskkill /FI "WINDOWTITLE eq DockerInstance" /T /F
pause

cd /d C:\Users\Jaskuen\Documents\GitHub\DISTRIBUTED-PROGRAMMING\Valuator
docker compose down

cd /d C:\nginx
nginx -s stop
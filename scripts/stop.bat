@echo off
taskkill /FI "WINDOWTITLE eq ValuatorInstance1" /T /F
taskkill /FI "WINDOWTITLE eq ValuatorInstance2" /T /F
taskkill /FI "WINDOWTITLE eq RankInstance1" /T /F
taskkill /FI "WINDOWTITLE eq RankInstance2" /T /F
taskkill /FI "WINDOWTITLE eq EventsLoggerInstance" /T /F
taskkill /FI "WINDOWTITLE eq DockerInstance" /T /F
taskkill /FI "WINDOWTITLE eq NginxInstance" /T /F

cd /d "%~dp0.."
docker compose down

cd /d C:\nginx
nginx -s stop

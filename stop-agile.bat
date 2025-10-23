@echo off
echo Checking for running Agile processes...
tasklist /FI "IMAGENAME eq Agile*" /FO TABLE
echo.
echo Stopping Agile processes...
taskkill /FI "IMAGENAME eq Agile*" /F
echo.
echo Process completed.
pause





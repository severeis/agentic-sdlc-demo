@echo off
rem Checks that the site running in IIS on this server responds. Same checks as smoke.ps1.
rem Usage: smoke-iis.cmd [base url], default http://localhost
setlocal
set "BASE=%~1"
if "%BASE%"=="" set "BASE=http://localhost"
set FAILED=0
set TRIES=0

rem first request compiles the views, give it time
:wait
curl.exe -fsS --max-time 30 "%BASE%/" > nul 2>&1 && goto ready
set /a TRIES+=1
if %TRIES% GEQ 30 (echo Site did not start & exit /b 1)
ping -n 3 127.0.0.1 > nul
goto wait

:ready
call :check "/" "Audit dashboard"
call :check "/Claims" "Mercy Family Clinic"
call :check "/Claims/Details/1003" "IMPOSSIBLE_UNITS"
call :check "/Claims/Search?q=Riverbend" "Riverbend Behavioral Health"

if %FAILED% NEQ 0 exit /b 1
echo Smoke test passed
exit /b 0

:check
curl.exe -fsS --max-time 30 "%BASE%%~1" | findstr /c:"%~2" > nul
if errorlevel 1 (echo FAIL %~1 & set /a FAILED+=1) else (echo OK   %~1)
exit /b 0

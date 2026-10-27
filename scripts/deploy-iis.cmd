@echo off
rem Deploys the published site to IIS on this server. Run by the self-hosted runner in deploy.yml.
rem   deploy-iis.cmd backup              copy the live site to the backup folder
rem   deploy-iis.cmd deploy <folder>     take the app offline, mirror <folder> into the site, bring it back
rem   deploy-iis.cmd rollback            restore the backup into the site folder
rem Plain cmd on purpose: the server's PowerShell execution policy stays untouched.
setlocal
set "SITE=C:\inetpub\claimsaudit"
set "BACKUP=C:\inetpub\claimsaudit-previous"
set "OFFLINE=%SITE%\app_offline.htm"

if /i "%~1"=="backup" goto backup
if /i "%~1"=="deploy" goto deploy
if /i "%~1"=="rollback" goto rollback
echo Usage: deploy-iis.cmd backup ^| deploy ^<folder^> ^| rollback
exit /b 2

:backup
robocopy "%SITE%" "%BACKUP%" /MIR /NFL /NDL /NJH /NJS /NP
if %ERRORLEVEL% GEQ 8 (echo Backup failed & exit /b 1)
echo Backed up %SITE% to %BACKUP%
exit /b 0

:deploy
if not exist "%~2\Web.config" (echo No published site in "%~2" & exit /b 1)
call :mirror "%~2" "Deploying a new version, back in a few seconds."
exit /b %ERRORLEVEL%

:rollback
call :mirror "%BACKUP%" "Rolling back."
exit /b %ERRORLEVEL%

:mirror
rem app_offline.htm makes ASP.NET unload the app and release its files while they are replaced
> "%OFFLINE%" echo ^<h1^>%~2^</h1^>
ping -n 3 127.0.0.1 > nul
rem robocopy: 0-7 mean success (files copied, extras removed, ...), 8+ mean failure
robocopy "%~1" "%SITE%" /MIR /XF app_offline.htm /NFL /NDL /NJH /NJS /NP
set "RC=%ERRORLEVEL%"
del /f /q "%OFFLINE%"
if %RC% GEQ 8 (echo robocopy failed with code %RC% & exit /b 1)
echo Copied %~1 to %SITE%
exit /b 0

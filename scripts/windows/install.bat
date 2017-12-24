:: Author: Blowa <blowa@gamefield.fr>
:: Title: NosWings Install Server Script for Windows
:: Description: This script will launch NosWings emulator for Windows Operating Systems.
@echo OFF

:: Define messages constats
set WRONG_FOLDER=You must sart the install script at the root folder of Rhisis.
set EXIT_MESSAGE=Press any key to exit installation script.
set LOGIN_DIRECTORY=dist\bin\login
set MASTER_DIRECTORY=dist\bin\master
set MIGRATOR_DIRECTORY=dist\bin\migrator
set WORLD_DIRECTORY=dist\bin\world

:: Check if dotnet command exists
WHERE %DOTNET_COMMAND% >nul 2>nul
if %ERRORLEVEL% neq 0 (
    call:displayErrorMessage %DOTNET_NOT_FOUND%
)

:: Check if we are at the root folder of Rhisis project
set IS_DIRECTORY_VALID=1

if not exist bin set IS_DIRECTORY_VALID=0
if not exist srcs set IS_DIRECTORY_VALID=0

if %IS_DIRECTORY_VALID% equ 0 (
    call:displayErrorMessage %WRONG_FOLDER%
)

:: Create dist folder if not exists
if not exist dist\bin md dist\bin
if not exist %LOGIN_DIRECTORY% md %LOGIN_DIRECTORY%
if not exist %MIGRATOR_DIRECTORY% md %MIGRATOR_DIRECTORY%
if not exist %MASTER_DIRECTORY% md %MASTER_DIRECTORY%
if not exist %WORLD_DIRECTORY% md %WORLD_DIRECTORY%

:: Compile NosWings
%DOTNET_COMMAND% restore
%DOTNET_COMMAND% build srcs\Rhisis.Core\ --configuration Release
%DOTNET_COMMAND% build srcs\Rhisis.Database\ --configuration Release
%DOTNET_COMMAND% build srcs\Rhisis.Login\ --configuration Release
%DOTNET_COMMAND% build srcs\Rhisis.Cluster\ --configuration Release
%DOTNET_COMMAND% build srcs\Rhisis.World\ --configuration Release

:: Copy binaries to dist\bin folders
xcopy /E srcs\Rhisis.Login\bin\Release\ %LOGIN_DIRECTORY%
xcopy /E srcs\Rhisis.Login\bin\Release\ %MIGRATOR_DIRECTORY%
xcopy /E srcs\Rhisis.Cluster\bin\Release\ %MASTER_DIRECTORY%
xcopy /E srcs\Rhisis.World\bin\Release\ %WORLD_DIRECTORY%

:: Copy start scripts to dist folder
xcopy script\windows\login-server.bat dist\
xcopy script\windows\cluster-server.bat dist\
xcopy script\windows\world-server.bat dist\

:: End of the script, goto the End Of File
goto EOF

:: Useful methods
:displayErrorMessage
echo %~1
echo %EXIT_MESSAGE%
pause > nul
exit

:EOF
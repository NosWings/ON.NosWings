:: Author: Blowa <blowa@gamefield.fr>
:: Title: NosWings Migrator Script for Windows
:: Description: This script will launch NosWings Database Migrator for Windows Operating Systems.
@echo OFF

:: Define constants
set DIRECTORY=dist\bin\migrator
set EXECUTABLE_NAME=NosWings.Migrator.exe

%DIRECTORY%\%EXECUTABLE_NAME%

:EOF
@echo off
SET DEBUG_PATH=%CD%\bin\Debug\netcoreapp3.1
SET NETCOREUTILS_ENVIRONMENT=Development

dotnet %DEBUG_PATH%\TestApp.Cli.Database.dll %*

:: or set the debug path to system path, then use the following statement to
:: replace the above one:
:: 
:: dotnet %~dp0TestApp.Cli.dll %*
@echo on
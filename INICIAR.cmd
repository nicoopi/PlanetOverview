@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0start.ps1"
if errorlevel 1 (
    echo.
    echo Nao foi possivel iniciar. Consulte LEIA-ME-GRUPO.md e a mensagem acima.
    pause
)

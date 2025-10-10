@echo off
echo === Testing Comments API ===
echo.
echo Make sure the backend is running on http://localhost:5175
echo.
pause
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0test-comments-api.ps1"
pause

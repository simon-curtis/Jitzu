@echo off
set "EXT_DIR=%USERPROFILE%\.vscode\extensions\jitzu.lang-0.0.1"

if not exist "%EXT_DIR%" mkdir "%EXT_DIR%"
xcopy /s /e /h /i /y "%cd%\*" "%EXT_DIR%\"
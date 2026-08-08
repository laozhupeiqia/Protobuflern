@echo off
rem One-click generate protobuf C# classes.
rem Reads all *.proto in this folder, outputs to ..\Generated (= Protos\Generated).
rem After editing a proto, double-click this, then build the server project.
cd /d "%~dp0"

rem protoc.exe comes from the Grpc.Tools package (nuget cache, present after first build).
set "PROTOC=%USERPROFILE%\.nuget\packages\grpc.tools\2.83.0\tools\windows_x64\protoc.exe"

if not exist "%PROTOC%" (
    echo [ERROR] protoc.exe not found: %PROTOC%
    echo Build the server project once so Grpc.Tools downloads it, then rerun this.
    exit /b 1
)

if not exist "..\Generated" mkdir "..\Generated"

for %%f in (*.proto) do (
    echo Generating %%f ...
    "%PROTOC%" --csharp_out=..\Generated "%%f"
    if errorlevel 1 exit /b 1
)

echo Done. Output is in Protos\Generated

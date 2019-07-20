# Setup path 
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDir = Split-Path $scriptPath
Push-Location $scriptDir
[Environment]::CurrentDirectory = $PWD
"Current directory at start of build.ps1 is: $PWD"

# Build (from app directory)
cd ClockWeatherDisplay.App

#dotnet build --runtime win10-arm --configuration release --output out

dotnet publish --runtime win10-arm --configuration release 

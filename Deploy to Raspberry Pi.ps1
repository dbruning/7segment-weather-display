# Setup path 
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDir = Split-Path $scriptPath
Push-Location $scriptDir
[Environment]::CurrentDirectory = $PWD

# Build (from app directory)
cd ClockWeatherDisplay.App

# Params
$DeviceIPAddress = "172.16.0.108" # You can get this from IoT Dashboard app
"Using device IP address: $DeviceIPAddress"

"Going to establish powershell session"
$session = New-PSSession –ComputerName $DeviceIPAddress -Credential $DeviceIPAddress\Administrator

"Going to copy app files"
Copy-Item -Path 'bin\release\netcoreapp2.1\win10-arm\publish\*.*' -Destination C:\programs\weather -ToSession $session

"Done!"
#
#dotnet build --runtime win10-arm --configuration release --output out 
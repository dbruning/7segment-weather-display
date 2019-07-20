# This script establishes a trust relationship with an IoT device, so that we can do stuff to it via PowerShell from dev pc.
# Following: 
# https://docs.microsoft.com/en-us/windows/iot-core/connect-your-device/powershell
# NOTE: this needs to be run as Administrator

# Params
$DeviceIPAddress = "172.16.0.108" # You can get this from IoT Dashboard app

# Setup path 
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDir = Split-Path $scriptPath
Push-Location $scriptDir
[Environment]::CurrentDirectory = $PWD

cd ClockWeatherDisplay.App

# Make sure WinRM is running on this computer (will hang silently without this if not)
"Going to start the local WinRM service"
Start-Service WinRM

# Trust the device
"Going to trust the device"
Set-Item WSMan:\localhost\Client\TrustedHosts -Value $DeviceIPAddress -force
"Trusted the device"

"You should now be able to start a remote powershell session with this command:"
#"Enter-PSSession -ComputerName $DeviceIPAddress -Credential $DeviceIPAddress\Administrator"


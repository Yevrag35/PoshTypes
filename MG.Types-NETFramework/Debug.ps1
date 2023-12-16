$curDir = Split-Path -Parent $MyInvocation.MyCommand.Definition;
$myDesktop = [System.Environment]::GetFolderPath("Desktop")

Import-Module "$curDir\MG.Types.dll" -ErrorAction Stop

Push-Location $([System.Environment]::GetFolderPath("Desktop"))
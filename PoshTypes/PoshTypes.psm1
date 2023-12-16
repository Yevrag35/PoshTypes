if ($PSVersionTable.PSVersion.Major -le 5) {
	
	# .NET Framework 4.7.1 + higher (Windows PowerShell 5.1)
	$dllPath = "$PSScriptRoot\DotNet\MG.Types.dll"
}
elseif ($PSVersionTable.PSVersion.Minor -ge 4) {
	
	# .NET 8 (PowerShell Version 7.4 and higher)
	$dllPath = "$PSScriptRoot\Core\net8.0\MG.Types.dll"
}
else {
	
	# .NET 6 (PowerShell Versions 7.2 and 7.3)
	$dllPath = "$PSScriptRoot\Core\net6.0\MG.Types.dll"
}

Import-Module $dllPath
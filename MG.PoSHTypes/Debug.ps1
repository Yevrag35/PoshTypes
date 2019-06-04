$curDir = Split-Path -Parent $MyInvocation.MyCommand.Definition;
Import-Module "$curDir\PoshTypes.psd1";

$list = New-Object -TypeName 'System.Collections.Generic.List[object]' -Property @{
	AddRange = [System.Collections.Generic.IEnumerable[object]]@("hi", "there", 1234, $(new-guid))
}
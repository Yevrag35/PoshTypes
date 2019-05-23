$code = @'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class BaseObject
{
    private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;

    internal object _backingField;

    internal void SetProperties<T>(T obj)
    {
        Type tt = typeof(T);
        Type thisType = this.GetType();
        IEnumerable<PropertyInfo> origProps = tt.GetProperties(FLAGS).Where(x => x.CanRead);

        PropertyInfo[] thisProps = thisType.GetProperties(FLAGS);
        
        for (int i = 0; i < thisProps.Length; i++)
        {
            var prop = thisProps[i];
            foreach (var thatProp in origProps)
            {
                if (thatProp.Name.Equals(prop.Name))
                {
                    object oVal = thatProp.GetValue(obj);
                    prop.SetValue(this, oVal);
                    break;
                }
            }
        }
        _backingField = obj;
    }
}

public class PoshMethod : BaseObject
{
    public MethodAttributes Attributes { get; private set; }
    public CallingConventions CallingConvention { get; private set; }
    public bool ContainsGenericParameters { get; private set; }
    public IEnumerable<CustomAttributeData> CustomAttributes { get; private set; }
    public Type DeclaringType { get; private set; }
    public bool IsAbstract { get; private set; }
    public bool IsAssembly { get; private set; }
    public bool IsConstructor { get; private set; }
    public bool IsFamily { get; private set; }
    public bool IsFamilyAndAssembly { get; private set; }
    public bool IsFamilyOrAssembly { get; private set; }
    public bool IsFinal { get; private set; }
    public bool IsGenericMethod { get; private set; }
    public bool IsGenericMethodDefinition { get; private set; }
    public bool IsHideBySig { get; private set; }
    public bool IsPrivate { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsSecurityCritical { get; private set; }
    public bool IsSecuritySafeCritical { get; private set; }
    public bool IsSecurityTransparent { get; private set; }
    public bool IsSpecialName { get; private set; }
    public bool IsStatic { get; private set; }
    public bool IsVirtual { get; private set; }
    public MemberTypes MemberType { get; private set; }
    public int MetadataToken { get; private set; }
    public RuntimeMethodHandle MethodHandle { get; private set; }
    public MethodImplAttributes MethodImplementationFlags { get; private set; }
    public Module Module { get; private set; }
    public string Name { get; private set; }
    public Type ReflectedType { get; private set; }
    public ParameterInfo ReturnParameter { get; private set; }
    public Type ReturnType { get; private set; }
    public ICustomAttributeProvider ReturnTypeCustomAttributes { get; private set; }

    private PoshMethod(MethodInfo mi)
    {
        base.SetProperties(mi);
    }

    public static implicit operator PoshMethod(MethodInfo mi)
    {
        return new PoshMethod(mi);
    }
    public static implicit operator MethodInfo(PoshMethod pm)
    {
        return (MethodInfo)pm._backingField;
    }
}

public class PoshMethodParameter : BaseObject
{
    public ParameterAttributes Attributes { get; private set; }
    public IEnumerable<CustomAttributeData> CustomAttributes { get; private set; }
    public object DefaultValue { get; private set; }
    public bool HasDefaultValue { get; private set; }
    public bool IsIn { get; private set; }
    public bool IsLcid { get; private set; }
    public bool IsOptional { get; private set; }
    public bool IsOut { get; private set; }
    public bool IsRetval { get; private set; }
    public MemberInfo Member { get; private set; }
    public int MetadataToken { get; private set; }
    public string Name { get; private set; }
    public Type ParameterType { get; private set; }
    public int Position { get; private set; }
    public object RawDefaultValue { get; private set; }

    private PoshMethodParameter(ParameterInfo pi)
    {
        base.SetProperties(pi);
    }

    public static implicit operator PoshMethodParameter(ParameterInfo pi)
    {
        return new PoshMethodParameter(pi);
    }
    public static implicit operator ParameterInfo(PoshMethodParameter pmp)
    {
        return (ParameterInfo)pmp._backingField;
    }
}
'@
Add-Type -TypeDefinition $code -Language CSharp -ReferencedAssemblies "System", "System.Collections", "System.Reflection";

Function Get-Type()
{
	[CmdletBinding(DefaultParameterSetName='None', PositionalBinding=$false)]
	[Alias("gt")]
	param
	(
		[parameter(Mandatory, Position = 0, ValueFromPipeline)]
		[object] $InputObject,
		
		[parameter(Mandatory, ParameterSetName='GetFullName')]
		[alias("f")]
        [switch] $FullName,
        
        [parameter(Mandatory, ParameterSetName='GetProperties')]
        [alias('p')]
        [switch] $Properties,

        [parameter(Mandatory, ParameterSetName='GetMethods')]
        [alias('m')]
        [switch] $Methods,

        [parameter(Mandatory=$false, ParameterSetName='GetProperties', DontShow)]
        [parameter(Mandatory=$false, ParameterSetName='GetMethods', DontShow)]
        [switch] $Force
	)
	Process
	{
        $type = $InputObject.GetType();
        $gmArgs = @{ Force = $Force.ToBool() }

        switch ($PSCmdlet.ParameterSetName)
        {
            "GetFullName"
            {
                $return = $type.FullName;
            }
            "GetProperties"
            {
                $return = Get-Member -InputObject $InputObject -MemberType Properties @gmArgs;
            }
            "GetMethods"
            {
                $return = Get-Member -InputObject $InputObject -MemberType Methods @gmArgs;
            }
            default
            {
                $return = $type;
            }
        }
        Write-Output -InputObject $return;
	}
}

Function Get-Parameter()
{
    [CmdletBinding(PositionalBinding = $false)]
    [Alias("gpm", "pm")]
    param
    (
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Reflection.MethodInfo] $Method,

        [parameter(Mandatory=$false, Position = 0)]
        [string[]] $Name
    )
    Begin
    {
        $eaArgs = @{}
        if ($PSBoundParameters.ContainsKey("ErrorAction"))
        {
            $eaArgs.ErrorAction = $PSBoundParameters["ErrorAction"];
        }
        $list = New-Object 'System.Collections.Generic.List[System.Reflection.ParameterInfo]'
        # [string[]]$enums = foreach($e in $Flags)
        # {
        #     $e.ToString()
        # };
        # [System.Reflection.BindingFlags]$bfs = [enum]::Parse($([System.Reflection.BindingFlags]), ($enums -join ','), $true);
    }
    Process
    {
        $allParams = $Method.GetParameters();
        if ($PSBoundParameters.ContainsKey("Name"))
        {
            $allParams = $allParams | Where-Object { $_.Name -in $Name };
        }
        $list.AddRange([System.Reflection.ParameterInfo[]]@($allParams));
    }
    End
    {

    }
}

Function Resolve-Type()
{
    [CmdletBinding()]
    [Alias("rt")]
    [OutputType([type])]
    param
    (
        [parameter(Mandatory, Position = 0, ValueFromPipeline)]
        [string[]] $TypeName
    )
    Begin
    {
        $baseStr = "[{0}]"
        $types = New-Object 'System.Collections.Generic.List[type]';
        $strs = New-Object System.Collections.Generic.List[string];
    }
    Process
    {
        $strs.AddRange($TypeName);
    }
    End
    {
        for ($i = 0; $i -lt $strs.Count; $i++)
        {
            $str = $strs[$i];
            try
            {
                [type]$result = Invoke-Expression -Command $($baseStr -f $str) -ErrorAction Stop;
                $types.Add($result);
            }
            catch [System.Management.Automation.RuntimeException]
            {
                if ($_.Exception.Message.Contains("Unable to find type"))
                {
                    $ea = $PSBoundParameters.ContainsKey("ErrorAction") -and $PSBoundParameters["ErrorAction"] -eq "Stop";
                    $maybe = [type]::GetType($str, $ea, $true);
                    if ($null -ne $maybe)
                    {
                        $types.Add($maybe);
                    }
                }
            }
        }
        Write-Output -InputObject $types.ToArray();
    }
}

Export-ModuleMember -Function "Get-Type", "Get-Parameter", "Resolve-Type" -Alias "gt"
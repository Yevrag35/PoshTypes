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
        base._backingField = mi;
    }

    public PoshMethodParameter[] GetParameters()
    {
        var prms = ((MethodInfo)_backingField).GetParameters();
        if (prms.Length > 0)
        {
            var newArr = new PoshMethodParameter[prms.Length];
            for (int i = 0; i < prms.Length; i++)
            {
                newArr[i] = prms[i];
            }
            return newArr;
        }
        else
        {
            return null;
        }
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
        base._backingField = pi;
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

public class PoshProperty : BaseObject
{
    public PropertyAttributes Attributes { get; private set; }
    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }
    public IEnumerable<CustomAttributeData> CustomAttributes { get; private set; }
    public Type DeclaringType { get; private set; }
    public MethodInfo GetMethod { get; private set; }
    public bool IsSpecialName { get; private set; }
    public MemberTypes MemberType { get; private set; }
    public int MetadataToken { get; private set; }
    public Module Module { get; private set; }
    public string Name { get; private set; }
    public Type PropertyType { get; private set; }
    public Type ReflectedType { get; private set; }
    public MethodInfo SetMethod { get; private set; }

    private PoshProperty(PropertyInfo pi)
    {
        base.SetProperties(pi);
        _backingField = pi;
    }

    public MethodInfo[] GetAccessors()
    {
        return ((PropertyInfo)_backingField).GetAccessors();
    }

    public MethodInfo[] GetAccessors(bool nonPublic)
    {
        return ((PropertyInfo)_backingField).GetAccessors(nonPublic);
    }

    public static implicit operator PoshProperty(PropertyInfo pi)
    {
        return new PoshProperty(pi);
    }
    public static implicit operator PropertyInfo(PoshProperty pp)
    {
        return (PropertyInfo)pp._backingField;
    }
}
'@
Add-Type -TypeDefinition $code -Language CSharp -ReferencedAssemblies "System", "System.Collections", "System.Linq", "System.Reflection";

Function Get-Type()
{
	[CmdletBinding(DefaultParameterSetName='GetTypeFromName', PositionalBinding=$false)]
    [Alias("gt")]
    [OutputType([type])]
	param
	(
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName='GetTypeFromPipeline')]
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName='GetFullNameFromPipeline')]
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName='GetPropertiesFromPipeline')]
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName='GetMethodsFromPipeline')]
        [object] $InputObject,
        
        [parameter(Mandatory, Position = 0, ParameterSetName='GetTypeFromName')]
        [parameter(Mandatory, Position = 0, ParameterSetName='GetFullNameFromName')]
        [parameter(Mandatory, Position = 0, ParameterSetName='GetPropertiesFromName')]
        [parameter(Mandatory, Position = 0, ParameterSetName='GetMethodsFromName')]
        [string[]] $TypeName,
		
        [parameter(Mandatory, ParameterSetName='GetFullNameFromPipeline')]
        [parameter(Mandatory, ParameterSetName='GetFullNameFromName')]
		[alias("f")]
        [switch] $FullName,
        
        [parameter(Mandatory, ParameterSetName='GetPropertiesFromPipeline')]
        [alias('p')]
        [switch] $Properties,

        [parameter(Mandatory, ParameterSetName='GetMethodsFromPipeline')]
        [alias('m')]
        [switch] $Methods,

        [parameter(Mandatory=$false, ParameterSetName='GetPropertiesFromPipeline', DontShow)]
        [parameter(Mandatory=$false, ParameterSetName='GetMethodsFromPipeline', DontShow)]
        [switch] $Force
	)
	Process
	{
        if ($PSBoundParameters.ContainsKey("InputObject"))
        {
            if ($InputObject -is [type])
            {
                [type[]]$ts = @(Resolve-Type -TypeName $InputObject.FullName)
            }
            else
            {
                [type[]]$ts = @($InputObject.GetType());
            }
        }
        else
        {
            [type[]]$ts = Resolve-Type -TypeName $TypeName;
        }
        $gmArgs = @{ Force = $Force.ToBool() }

        foreach ($type in $ts)
        {
            Write-Debug ("ParameterSetName is: {0}" -f $PSCmdlet.ParameterSetName);
            Write-Debug "Current Type: $($type.FullName)"
            switch ($PSCmdlet.ParameterSetName)
            {
                "GetFullNameFromPipeline"
                {
                    $return = $type.FullName;
                }
                "GetFullNameFromName"
                {
                    $return = $type.FullName;
                }
                "GetPropertiesFromPipeline"
                {
                    $return = Get-Member -InputObject $InputObject -MemberType Properties @gmArgs;
                }
                "GetPropertiesFromName"
                {
                    $return = Get-Member -InputObject $InputObject -MemberType Properties @gmArgs;
                }
                "GetMethodsFromPipeline"
                {
                    $return = Get-Member -InputObject $InputObject -MemberType Methods @gmArgs;
                }
                "GetMethodsFromName"
                {
                    $return = Get-Member -InputObject $InputObject -MemberType Methods @gmArgs;
                }
                default
                {
                    $return = $type;
                }
            }
        }
        Write-Output -InputObject $return;
	}
}

Function Get-Method()
{
    [CmdletBinding(PositionalBinding=$false, DefaultParameterSetName='ByPipelineType')]
    [Alias("gmt")]
    [OutputType([PoshMethod])]
    param
    (
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName='ByPipelineType')]
        [type] $InputObject,

        [parameter(Mandatory, ParameterSetName="ByTypeName")]
        [string] $TypeName,

        [parameter(Mandatory = $false, Position = 0)]
        [string[]] $Name,

        [parameter(Mandatory = $false)]
        [System.Reflection.BindingFlags[]] $Flags = @([System.Reflection.BindingFlags]::Public,[System.Reflection.BindingFlags]::Instance),

        [parameter(Mandatory = $false)]
        [switch] $Force
    )
    Begin
    {
        $list = New-Object System.Collections.Generic.List[type];
        $realFlags = Join-Enum -Flags $Flags;
    }
    Process
    {
        if ($PSBoundParameters.ContainsKey("InputObject"))
        {
            $list.Add($InputObject)
        }
        else
        {
            $list.Add((Resolve-Type -TypeName $TypeName));
        }
    }
    End
    {
        $outList = New-Object System.Collections.Generic.List[PoshMethod]
        for ($i = 0; $i -lt $list.Count; $i++)
        {
            $t = $list[$i];
            [System.Reflection.MethodInfo[]]$allMethods = $t.GetMethods($realFlags);
            if ($PSBoundParameters.ContainsKey("Name"))
            {
                $script = { $_.Name -in $Name }
                [System.Reflection.MethodInfo[]]$allMethods = $allMethods | Where-Object $script;
            }
            elseif (-not $PSBoundParameters.ContainsKey("Force"))
            {
                $script = { $_.Name -notlike "*_*" }
                [System.Reflection.MethodInfo[]]$allMethods = $allMethods | Where-Object $script;
            }
            foreach ($meth in $allMethods)
            {
                $outList.Add($meth);
            }
        }
        Write-Output $outList;
    }
}

Function Get-Property()
{
    [CmdletBinding(PositionalBinding = $false)]
    [Alias("getprop", "gpt")]
    [OutputType([PoshProperty])]
    param
    (
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName='ByPipelineType')]
        [type] $InputObject,

        [parameter(Mandatory, ParameterSetName='ByTypeName')]
        [string] $TypeName,

        [parameter(Mandatory = $false, Position = 1)]
        [string[]] $Name,

        [parameter(Mandatory = $false, Position = 0)]
        [ValidateSet("All", "Get", "GetSet", "Set")]
        [string] $Accessors = "All",

        [parameter(Mandatory = $false)]
        [System.Reflection.BindingFlags[]] $Flags = @([System.Reflection.BindingFlags]::Public, [System.Reflection.BindingFlags]::Instance)
    )
    Begin
    {
        $list = New-Object System.Collections.Generic.List[type];
        $realFlags = Join-Enum -Flags $Flags;
    }
    Process
    {
        if ($PSBoundParameters.ContainsKey("InputObject"))
        {
            $list.Add($InputObject);
        }
        else
        {
            $list.Add((Resolve-Type -TypeName $TypeName));
        }
    }
    End
    {
        $outList = New-Object System.Collections.Generic.List[PoshProperty];
        for ($i = 0; $i -lt $list.Count; $i++)
        {
            $t = $list[$i];
            [System.Reflection.PropertyInfo[]]$allProps = $t.GetProperties($realFlags);
            if ($PSBoundParameters.ContainsKey("Name"))
            {
                $script = { $_.Name -in $Name }
                [System.Reflection.PropertyInfo[]]$allProps = $allProps | Where-Object $script;
            }
            if ($PSBoundParameters.ContainsKey("Accessors") -and $Accessors -ne "All")
            {
                switch ($Accessors)
                {
                    "GetSet"
                    {
                        $script = { $_.CanRead -and $_.CanWrite };
                    }
                    "Set"
                    {
                        $script = { -not $_.CanRead -and $_.CanWrite };
                    }
                    default
                    {
                        $script = { $_.CanRead };
                    }
                }
                [System.Reflection.PropertyInfo[]]$allProps = $allProps | Where-Object $script;
            }
            foreach ($prop in $allProps)
            {
                $outList.Add($prop);
            }
        }
        Write-Output $outList;
    }
}

Function Get-Parameter()
{
    [CmdletBinding(PositionalBinding = $false, DefaultParameterSetName='ByRealMethod')]
    [Alias("gpm", "pm")]
    [OutputType([PoshMethodParameter])]
    param
    (
        [parameter(Mandatory, ValueFromPipeline, ParameterSetName='ByRealMethod')]
        [System.Reflection.MethodInfo] $Method,

        # [parameter(Mandatory, ValueFromPipeline, ParameterSetName='ByMyMethod')]
        # [PoshMethod] $PoshMethod,

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
        $list = New-Object 'System.Collections.Generic.List[PoshMethodParameter]'
        # [string[]]$enums = foreach($e in $Flags)
        # {
        #     $e.ToString()
        # };
        # [System.Reflection.BindingFlags]$bfs = [enum]::Parse($([System.Reflection.BindingFlags]), ($enums -join ','), $true);
    }
    Process
    {
        [System.Reflection.ParameterInfo[]]$allParams = $Method.GetParameters();
        if ($PSBoundParameters.ContainsKey("Name"))
        {
            [System.Reflection.ParameterInfo[]]$allParams = $allParams | Where-Object { $_.Name -in $Name };
        }
        for ($i = 0; $i -lt $allParams.Length; $i++)
        {
            $list.Add([PoshMethodParameter]$allParams[$i]);
        }
    }
    End
    {
        Write-Output $list;
    }
}

#region BACKEND FUNCTIONS

Function Resolve-Type()
{
    [CmdletBinding()]
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

Function Join-Enum([System.Reflection.BindingFlags[]]$Flags)
{
    [string[]]$strFlags = @();
    foreach ($e in $Flags)
    {
        $strFlags += $e.ToString();
    }
    Write-Output -InputObject $([enum]::Parse(([System.Reflection.BindingFlags]), ($strFlags -join ','), $true));
}

#endregion

$export = @{
    Function = @(
        "Get-Method",
        "Get-Parameter",
        "Get-Property",
        "Get-Type"
    )
    Alias = @(
        "gmt",
        "gpm",
        "gpt",
        "gt",
        "pm"
    )
};

Export-ModuleMember @export;
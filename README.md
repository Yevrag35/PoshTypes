# PoshTypes
[![version](https://img.shields.io/powershellgallery/v/PoshTypes.svg)](https://www.powershellgallery.com/packages/PoshTypes)
[![downloads](https://img.shields.io/powershellgallery/dt/PoshTypes.svg?label=downloads)](https://www.powershellgallery.com/stats/packages/PoshTypes?groupby=Version)

A module that I've written because I've determined that I can't be bothered to type '.GetType()' and other various commands when working in the console.

This is all written **WITHOUT** affecting the default Type or Format data for `System.RuntimeType` and other built-in types and, instead, uses custom PSObject types.  Not only does this give unique, formatted output but each cmdlet's output is also a usable version of its wrapped object.

In order words...
```powershell
$type = Get-Type [System.Collections.Generic.List[object]]
$type -is [type]
# True

$method = Get-Method [System.Collections.ArrayList] -Name Clear
$method -is [System.Reflection.MethodInfo]
# True
```

### Get-PSType

A little bit more flexible cmdlet than `Get-Member`.

#### How to use
Get the type straight from a string or object depending if you type it manually, use a variable, or use the pipeline.
```powershell

# From just a type expression string:
Get-Type [guid]

#     ParentType: System.Object
#
# ReflectionType Name  PSName
# -------------- ----  ------
#         Struct Guid  guid

# From a variable without enumeration:
$array = @(1, 2, 3)
Get-Type $array

#     ParentType: System.Array
#
# ReflectionType Name     PSName
# -------------- ----     ------
#          Class Object[] System.Object[]

# From each item in the pipeline:
$array = @(1, 2, 3)
$array | Get-Type

#     ParentType: System.ValueType
#
# ReflectionType Name   PSName
# -------------- ----   ------
#         Struct Int32  int

```

### Get-PSMethod

Returns methods on specified .NET types.

```powershell
# Get from the specified type:
Get-Method [hashtable] -Name c*
# -or-
# Pipe in from 'Get-PSType'
Get-Type [hashtable] | Get-Method c*

#   ImplementingType: System.Collections.Hashtable
#
# Name          ReturnType     Definition
# ----          ----------     ----------
# Clear         System.Void    void Clear()
# Clone         System.Object  System.Object Clone()
# Contains      System.Boolean bool Contains(System.Object key)
# ContainsKey   System.Boolean bool ContainsKey(System.Object key)
# ContainsValue System.Boolean bool ContainsValue(System.Object value)
# CopyTo        System.Void    void CopyTo(array array, int arrayIndex)

```
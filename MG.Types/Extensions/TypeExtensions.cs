using MG.Types.Components;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Reflection;

namespace MG.Types.Extensions
{
    public static class TypeExtensions
    {
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(type))]
        public static string? GetFullAssemblyTypeName(this Type? type)
        {
            if (type is null)
            {
                return null;
            }

            return type.FullName ?? string.Empty;
        }

#if NET6_0_OR_GREATER
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(type))]
        public static string? GetPSTypeName(this Type? type)
        {
            return GetPSTypeName(type, removeBrackets: true);
        }

        [return: NotNullIfNotNull(nameof(type))]
        public static string? GetPSTypeName(this Type? type, bool removeBrackets)
        {
            if (type is null)
            {
                return null;
            }

            string? name = LanguagePrimitives.ConvertTypeNameToPSTypeName(GetTypeName(type));

            if (string.IsNullOrWhiteSpace(name))
            {
                return type.FullName ?? type.Name;
            }
            else if (!removeBrackets || !name.StartsWith('[') || !name.EndsWith(']'))
            {
                return name;
            }

            Span<char> span = stackalloc char[name.Length];
            name.AsSpan(1).CopyTo(span);
            span = span.Slice(0, name.Length - 2);

            return new string(span);
        }

        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(type))]
        public static string? GetTypeName(this Type? type)
        {
            if (type is null)
            {
                return null;
            }

            int? nsLength = type.Namespace?.Length;
            int length = type.Name.Length;
            if (nsLength.HasValue)
            {
                length += 1 + nsLength.Value;
            }

            return string.Create(length, type, (chars, state) =>
            {
                ReadOnlySpan<char> ns = state.Namespace.AsSpan();
                int position = 0;
                if (!ns.IsEmpty)
                {
                    ns.CopyTo(chars);
                    position += ns.Length;
                    chars[position++] = '.';
                }

                state.Name.AsSpan().CopyTo(chars.Slice(position));
            });
        }
#else
        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(type))]
        public static string? GetTypeName(this Type? type)
        {
            if (type is null)
            {
                return null;
            }
            else if (string.IsNullOrEmpty(type.Namespace))
            {
                return type.Name;
            }
            else
            {
                return string.Concat(type.Namespace, '.', type.Name);
            }
        }
#endif
    }
}


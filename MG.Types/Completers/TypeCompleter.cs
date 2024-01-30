using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace MG.Types.Completers
{
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public sealed class TypeCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            if (string.IsNullOrEmpty(wordToComplete) || !wordToComplete.StartsWith('[') || wordToComplete.EndsWith(']'))
            {
                return Enumerable.Empty<CompletionResult>();
            }

            return Enumerable.Repeat(new CompletionResult(string.Concat('(', wordToComplete)), 1);
        }
    }
}


using MG.Types.Extensions;
using System;
using System.Management.Automation.Language;
using System.Management.Automation;
using MG.Types.PSObjects;
using System.Diagnostics.CodeAnalysis;

namespace MG.Types.Attributes
{
    internal sealed class ArgumentToTypeTransformAttribute : ArgumentTransformationAttribute
    {
        const string PSREADLINE = "PSReadLine";

        public override object? Transform(EngineIntrinsics engineIntrinsics, object? inputData)
        {
            object? target = inputData.GetBaseObject();

            return GetPSTypeObject(engineIntrinsics, target);
        }

        private static Type? GetPSTypeObject(EngineIntrinsics engineIntrinsics, object? target)
        {
            switch (target)
            {
                case Type type:
                    return type;

                case ScriptBlock block:
                    return ResolveFromAst(block.Ast, engineIntrinsics.SessionState.Module);

                case string typeName:
                    return ResolveFromName(typeName, engineIntrinsics.SessionState.Module);

                case null:
                    return null;

                default:
                    return target.GetType();
            }
        }

        private static Type ResolveFromAst(Ast ast, PSModuleInfo? runningModule)
        {
            try
            {
                var first = (TypeExpressionAst?)ast.Find(x => x is TypeExpressionAst, false);

                return GetTypeFromAstOrObject(ast, first);
            }
            catch (ParseException e)
            {
                if (!(runningModule is null) && PSREADLINE.Equals(runningModule.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return typeof(object);
                }

                throw new ArgumentException($"'{ast.Extent.Text}' is not a valid .NET or custom-defined type.", e);
            }
        }

        private static Type GetTypeFromAstOrObject(Ast ast, TypeExpressionAst? first)
        {
            if (first is null)
            {
                return typeof(string);
            }

            return first.TypeName.GetReflectionType() ?? ThrowNotType(ast);
        }

        [DoesNotReturn]
        private static Type ThrowNotType(Ast ast)
        {
            throw new ParseException($"{ast.Extent.Text} is not a type expression.");
        }

        private static Type ResolveFromName(string typeName, PSModuleInfo? runningModule)
        {
            Ast ast;

            try
            {
                ast = Parser.ParseInput(typeName, out Token[] tokens, out ParseError[] errors);

                if (!(errors is null) && errors.Length > 0)
                {
                    throw new ParseException(errors);
                }
            }
            catch (ParseException e)
            {
                if (!(runningModule is null) && PSREADLINE.Equals(runningModule.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return typeof(object);
                }

                throw new ArgumentException($"'{typeName}' is not a valid .NET or custom-defined type.", e);
            }

            return ResolveFromAst(ast, runningModule);
        }
    }
}


// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Core.Diagnostics;
using Bicep.Core.Extensions;
using Bicep.Core.Semantics;
using Bicep.Core.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bicep.Core.Analyzers.Linter.Rules
{
    public sealed class NoUnusedParametersRule : NoUnusedRuleBase
    {
        public new const string Code = "no-unused-params";

        public NoUnusedParametersRule() : base(
            code: Code,
            description: CoreResources.ParameterMustBeUsedRuleDescription,
            docUri: new Uri($"https://aka.ms/bicep/linter/{Code}"),
            diagnosticStyling: DiagnosticStyling.ShowCodeAsUnused)
        { }

        public override string FormatMessage(params object[] values)
        {
            return string.Format(CoreResources.ParameterMustBeUsedRuleMessageFormat, values);
        }

        override public IEnumerable<IDiagnostic> AnalyzeInternal(SemanticModel model, DiagnosticLevel diagnosticLevel)
        {
            var invertedBindings = model.Binder.Bindings.ToLookup(kvp => kvp.Value, kvp => kvp.Key);

            // VariableAccessSyntax indicates a reference to the parameter
            var unreferencedParams = model.Root.ParameterDeclarations
                .Where(sym => sym.NameSyntax.IsValid)
                .Where(sym => !invertedBindings[sym].Any(x => x != sym.DeclaringSyntax));

            foreach (var param in unreferencedParams)
            {
                yield return CreateRemoveUnusedDiagnosticForSpan(diagnosticLevel, param.Name, param.NameSyntax, param.DeclaringSyntax, model.SourceFile.ProgramSyntax);
            }
        }

        override protected string GetCodeFixDescription(string name)
        {
            return $"Remove unused parameter {name}";
        }
    }
}

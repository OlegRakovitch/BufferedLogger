using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BufferedLoggerAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BufferedLoggerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BufferedLoggerAnalyzer";

        static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        const string Category = "Usage";

        const string StaticModifierName = "static";
        static readonly string BufferedAttributeName = nameof(BufferedAttribute).Replace("Attribute", "");
        static readonly string FunctionNameAttributeName = nameof(FunctionNameAttribute).Replace("Attribute", "");
        static readonly string ILoggerTypeName = nameof(ILogger);
        

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.Parameter);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is ParameterSyntax parameter)
            {
                if (parameter.Type is IdentifierNameSyntax typeName)
                {
                    if (typeName.Identifier.ValueText == ILoggerTypeName)
                    {
                        var diagnostic = ProcessILoggerParameter(parameter);
                        if (diagnostic != null)
                        {
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static Diagnostic ProcessILoggerParameter(ParameterSyntax parameter)
        {
            var methodDeclaration = GetMethodDeclaration(parameter);
            if (methodDeclaration != null)
            {
                if (methodDeclaration.Modifiers.Any(attr => StaticModifierName.Equals(attr.ValueText)))
                {
                    var methodAttributes = methodDeclaration.AttributeLists.SelectMany(attr => attr.Attributes);
                    if (methodAttributes.Any(attr => FunctionNameAttributeName.Equals(GetAttributeTypeName(attr))))
                    {
                        var attributes = parameter.AttributeLists.SelectMany(attr => attr.Attributes);
                        if (!attributes.Any(attr => BufferedAttributeName.Equals(GetAttributeTypeName(attr))))
                        {
                            return Diagnostic.Create(Rule, parameter.Identifier.GetLocation());
                        }
                    }
                }
            }
            return null;
        }

        static string GetAttributeTypeName(AttributeSyntax attributeSyntax)
        {
            if (attributeSyntax.Name is IdentifierNameSyntax name)
            {
                return name.Identifier.ValueText;
            }
            else
            {
                return string.Empty;
            }
        }

        static MethodDeclarationSyntax GetMethodDeclaration(SyntaxNode node)
        {
            while (node != null)
            {
                if (node is MethodDeclarationSyntax syntax)
                {
                    return syntax;
                }
                else
                {
                    node = node.Parent;
                }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace BufferedLoggerAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BufferedLoggerAnalyzerCodeFixProvider)), Shared]
    public class BufferedLoggerAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add [Buffered] attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(BufferedLoggerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var rootTokenAncestors = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf();
            var token = rootTokenAncestors.OfType<ParameterSyntax>().Single();
            var declaration = rootTokenAncestors.OfType<DeclarationExpressionSyntax>().SingleOrDefault();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    c => AddBufferedAttribute(context.Document, token, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> AddBufferedAttribute(Document document, ParameterSyntax parameter, CancellationToken cancellationToken)
        {
            var newAttribute = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Buffered"), null)));
            var whitespaceTrivia = parameter.GetLeadingTrivia().Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)).ToSyntaxTriviaList();
            var newParameter = parameter.WithAttributeLists(parameter.AttributeLists.Add(newAttribute)).WithLeadingTrivia(whitespaceTrivia);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(parameter, newParameter);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}

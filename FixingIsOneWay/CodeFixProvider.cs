using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FixingIsOneWay
{
	[ExportCodeFixProvider(IsOneWayOperationConstants.DiagnosticId, LanguageNames.CSharp)]
	public sealed class IsOneWayOperationMakeIsOneWayFalseCodeFix
		: ICodeFixProvider
	{
		public IEnumerable<string> GetFixableDiagnosticIds()
		{
			return new[] { IsOneWayOperationConstants.DiagnosticId };
		}

		public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
			IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (cancellationToken.IsCancellationRequested)
			{
				return Enumerable.Empty<CodeAction>();
			}

			var diagnostic = diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var attributeArgument = root.FindToken(diagnosticSpan.Start)
				.Parent.AncestorsAndSelf().OfType<AttributeArgumentSyntax>().First();

			if (cancellationToken.IsCancellationRequested)
			{
				return Enumerable.Empty<CodeAction>();
			}

			var trueToken = attributeArgument.Expression.GetFirstToken();

			var falseToken = SyntaxFactory.Token(trueToken.LeadingTrivia,
				SyntaxKind.FalseKeyword, trueToken.TrailingTrivia);

			var newRoot = root.ReplaceToken(trueToken, falseToken);

			return new[]
			{
				CodeAction.Create("Make IsOneWay = false", document.WithSyntaxRoot(newRoot))
			};
		}
	}
}
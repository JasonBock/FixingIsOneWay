using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace FixingIsOneWay
{
	[ExportCodeFixProvider(IsOneWayOperationConstants.DiagnosticId, LanguageNames.CSharp)]
	[Shared]
	public sealed class IsOneWayOperationMakeIsOneWayFalseCodeFix
		: CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(IsOneWayOperationConstants.DiagnosticId);
			}
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (context.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var attributeArgument = root.FindToken(diagnosticSpan.Start)
				.Parent.AncestorsAndSelf().OfType<AttributeArgumentSyntax>().First();

			if (context.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			var trueToken = attributeArgument.Expression.GetFirstToken();

			var falseToken = SyntaxFactory.Token(trueToken.LeadingTrivia,
				SyntaxKind.FalseKeyword, trueToken.TrailingTrivia);

			var newRoot = root.ReplaceToken(trueToken, falseToken);

			context.RegisterCodeFix(
				CodeAction.Create(
					IsOneWayOperationMakeIsOneWayFalseCodeFixConstants.Description,
					_ => Task.FromResult<Document>(context.Document.WithSyntaxRoot(newRoot))), diagnostic);
		}
	}
}
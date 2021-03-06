﻿using System.Composition;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FixingIsOneWay
{
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	[Shared]
	public sealed class IsOneWayOperationMakeIsOneWayFalseCodeFix
		: CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(IsOneWayOperationConstants.Id);
			}
		}

		public override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(
				context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics[0];
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var attributeArgument = root.FindToken(diagnosticSpan.Start)
				.Parent.AncestorsAndSelf().OfType<AttributeArgumentSyntax>().First();

			context.CancellationToken.ThrowIfCancellationRequested();

			var trueToken = attributeArgument.Expression.GetFirstToken();

			var falseToken = SyntaxFactory.Token(trueToken.LeadingTrivia,
				SyntaxKind.FalseKeyword, trueToken.TrailingTrivia);

			var newRoot = root.ReplaceToken(trueToken, falseToken);

			context.RegisterCodeFix(
				CodeAction.Create(
					IsOneWayOperationMakeIsOneWayFalseCodeFixConstants.Description,
					_ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
					IsOneWayOperationMakeIsOneWayFalseCodeFixConstants.Description),
				diagnostic);
		}
	}
}

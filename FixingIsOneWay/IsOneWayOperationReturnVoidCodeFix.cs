using System.Composition;
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
	public sealed class IsOneWayOperationReturnVoidCodeFix
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
			var model = await context.Document.GetSemanticModelAsync();

			var diagnostic = context.Diagnostics[0];
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var returnNode = root.FindToken(diagnosticSpan.Start)
				.Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First()
				.DescendantNodes().OfType<PredefinedTypeSyntax>().First();

			context.CancellationToken.ThrowIfCancellationRequested();

			var voidReturnNode = SyntaxFactory.PredefinedType( 
				SyntaxFactory.Token(returnNode.GetLeadingTrivia(),
					SyntaxKind.VoidKeyword, returnNode.GetTrailingTrivia()));

			var newRoot = root.ReplaceNode(returnNode, voidReturnNode);

			context.RegisterCodeFix(
				CodeAction.Create(
					IsOneWayOperationReturnVoidCodeFixConstants.Description,
					_ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
					IsOneWayOperationReturnVoidCodeFixConstants.Description),
				diagnostic);
		}
	}
}

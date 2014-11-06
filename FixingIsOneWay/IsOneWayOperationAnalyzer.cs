using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace FixingIsOneWay
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class IsOneWayOperationAnalyzer 
		: DiagnosticAnalyzer
	{
		private static DiagnosticDescriptor makeOneWayFalseRule = new DiagnosticDescriptor(
			IsOneWayOperationConstants.DiagnosticId, IsOneWayOperationConstants.Title,
			IsOneWayOperationConstants.Message, IsOneWayOperationConstants.Category, 
			DiagnosticSeverity.Error, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(IsOneWayOperationAnalyzer.makeOneWayFalseRule);
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction<SyntaxKind>(
				IsOneWayOperationAnalyzer.AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
		}

		private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
		{
			var methodNode = (MethodDeclarationSyntax)context.Node;

			if (methodNode.AttributeLists.Count > 0)
			{
				if (context.CancellationToken.IsCancellationRequested)
				{
					return;
				}

				foreach (var attribute in methodNode.AttributeLists)
				{
					foreach (var syntax in attribute.Attributes)
					{
						var attributeType = context.SemanticModel.GetTypeInfo(syntax).Type;

						if (attributeType != null &&
							attributeType.Name == IsOneWayOperationConstants.OperationContractTypeName &&
							attributeType.ContainingAssembly.Name == IsOneWayOperationConstants.OperationContractTypeAssemblyName)
						{
							foreach (var argument in syntax.ArgumentList.Arguments)
							{
								if (argument.NameEquals.Name.Identifier.Text == IsOneWayOperationConstants.IdentifierText &&
									argument.Expression.IsKind(SyntaxKind.TrueLiteralExpression))
								{
									if (context.CancellationToken.IsCancellationRequested)
									{
										return;
									}

									var returnType = context.SemanticModel.GetTypeInfo(methodNode.ReturnType).Type;

									if (returnType != null &&
										returnType.SpecialType != SpecialType.System_Void)
									{
										context.ReportDiagnostic(Diagnostic.Create(IsOneWayOperationAnalyzer.makeOneWayFalseRule,
											argument.GetLocation()));
										return;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}

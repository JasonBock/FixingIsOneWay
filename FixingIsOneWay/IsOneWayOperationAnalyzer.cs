using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ServiceModel;

namespace FixingIsOneWay
{
	[DiagnosticAnalyzer]
	[ExportDiagnosticAnalyzer("FindingIsOneWayOperationsWithReturnValues", LanguageNames.CSharp)]
	public sealed class IsOneWayOperationAnalyzer
		: ISyntaxNodeAnalyzer<SyntaxKind>
	{
		private static DiagnosticDescriptor makeOneWayFalseRule = new DiagnosticDescriptor(
			IsOneWayOperationConstants.DiagnosticId, IsOneWayOperationConstants.Description,
			IsOneWayOperationConstants.Message, "Usage", DiagnosticSeverity.Error, true);

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(IsOneWayOperationAnalyzer.makeOneWayFalseRule);
			}
		}

		public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
		{
			get
			{
				return ImmutableArray.Create(SyntaxKind.MethodDeclaration);
			}
		}

		public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
			AnalyzerOptions options, CancellationToken cancellationToken)
		{
			var methodNode = (MethodDeclarationSyntax)node;

			if (methodNode.AttributeLists.Count > 0)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				var operationContractType =
					typeof(OperationContractAttribute);

				foreach (var attribute in methodNode.AttributeLists)
				{
					foreach (var syntax in attribute.Attributes)
					{
						var attributeType = semanticModel.GetTypeInfo(syntax).Type;

						if (attributeType != null &&
							attributeType.Name ==
								operationContractType.Name &&
							attributeType.ContainingAssembly.Name ==
								operationContractType.Assembly.GetName().Name)
						{
							foreach (var argument in syntax.ArgumentList.Arguments)
							{
								if (argument.NameEquals.Name.Identifier.Text == IsOneWayOperationConstants.IdentifierText &&
									argument.Expression.IsKind(SyntaxKind.TrueLiteralExpression))
								{
									if (cancellationToken.IsCancellationRequested)
									{
										return;
									}

									var returnType = semanticModel.GetTypeInfo(methodNode.ReturnType).Type;

									if (returnType != null &&
										returnType.SpecialType != SpecialType.System_Void)
									{
										addDiagnostic(Diagnostic.Create(IsOneWayOperationAnalyzer.makeOneWayFalseRule,
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

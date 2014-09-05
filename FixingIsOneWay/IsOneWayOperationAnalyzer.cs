using System;
using System.Collections.Immutable;
using System.Linq;
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
		private static Lazy<DiagnosticDescriptor> makeOneWayFalseRule = new Lazy<DiagnosticDescriptor>(() =>
			new DiagnosticDescriptor(IsOneWayOperationConstants.DiagnosticId, IsOneWayOperationConstants.Description,
				IsOneWayOperationConstants.Message, "Usage", DiagnosticSeverity.Error, true));

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(IsOneWayOperationAnalyzer.makeOneWayFalseRule.Value);
			}
		}

		public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
		{
			get
			{
				return ImmutableArray.Create(SyntaxKind.MethodDeclaration);
			}
		}

		public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
		{
			var methodNode = (MethodDeclarationSyntax)node;

			if (methodNode.AttributeLists.Count > 0)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				//foreach(var attribute in methodNode.AttributeLists)
				//{
				//	foreach(var syntax in attribute.Attributes)
				//	{
				//		var attributeType = semanticModel.GetTypeInfo(syntax).Type;

				//		if(attributeType != null &&
				//			attributeType.
				//	}
				//}

				var operationContractType =
					typeof(OperationContractAttribute);
				// TODO (in full disclosure): Undo the LINQ.
				var operationSyntax = (
					from attribute in methodNode.AttributeLists
					from syntax in attribute.Attributes
					let attributeType = semanticModel.GetTypeInfo(syntax).Type
					where
						attributeType != null &&
						attributeType.Name ==
							operationContractType.Name &&
						attributeType.ContainingAssembly.Name ==
							operationContractType.Assembly.GetName().Name
					from argument in syntax.ArgumentList.Arguments
					where (
						argument.NameEquals.Name.Identifier.Text ==
							"IsOneWay" &&
						argument.Expression.RawKind ==
							(int)SyntaxKind.TrueLiteralExpression)
					select new { syntax, argument }).FirstOrDefault();

				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				if (operationSyntax != null)
				{
					var returnType = semanticModel.GetTypeInfo(methodNode.ReturnType).Type;

					if (cancellationToken.IsCancellationRequested)
					{
						return;
					}

					if (returnType != null &&
						returnType.SpecialType != SpecialType.System_Void)
					{
						addDiagnostic(Diagnostic.Create(IsOneWayOperationAnalyzer.makeOneWayFalseRule.Value,
							operationSyntax.argument.GetLocation()));
					}
				}
			}
		}
	}
}

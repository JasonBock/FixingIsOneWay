using System.Collections.Immutable;
using System.Linq;
using System.ServiceModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Reflection;

namespace FixingIsOneWay
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class IsOneWayOperationAnalyzer
		: DiagnosticAnalyzer
	{
		public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
			IsOneWayOperationConstants.Id, IsOneWayOperationConstants.Title,
			IsOneWayOperationConstants.Message, IsOneWayOperationConstants.Category,
			IsOneWayOperationConstants.Severity, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(IsOneWayOperationAnalyzer.Descriptor);
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction<SyntaxKind>(
			  this.AnalyzeNode, SyntaxKind.MethodDeclaration);
		}

		public void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			var methodNode = (MethodDeclarationSyntax)context.Node;

			if (methodNode.AttributeLists.Count > 0)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var returnType = context.SemanticModel.GetTypeInfo(methodNode.ReturnType).Type;

				if (returnType != null &&
					returnType.SpecialType != SpecialType.System_Void)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					var operationContractType =
						typeof(OperationContractAttribute);
					var operationContractTypeName = operationContractType.Name;
					var operationContractTypeAssemblyName = operationContractType.GetTypeInfo().Assembly.GetName().Name;

					// TODO (in full disclosure): Undo the LINQ.
					var trueArgument = (
						from attribute in methodNode.AttributeLists
						from syntax in attribute.Attributes
						let attributeType = context.SemanticModel.GetTypeInfo(syntax).Type
						where
							attributeType != null &&
							attributeType.Name == operationContractTypeName &&
							attributeType.ContainingAssembly.Name == operationContractTypeAssemblyName
						from argument in syntax.ArgumentList.Arguments
						where
							argument.NameEquals.Name.Identifier.Text == "IsOneWay" &&
							argument.Expression.IsKind(SyntaxKind.TrueLiteralExpression)
						select argument).FirstOrDefault();

					if (trueArgument != null)
					{
						context.ReportDiagnostic(Diagnostic.Create(
						  IsOneWayOperationAnalyzer.Descriptor,
						  trueArgument.GetLocation()));
					}
				}
			}
		}
	}
}

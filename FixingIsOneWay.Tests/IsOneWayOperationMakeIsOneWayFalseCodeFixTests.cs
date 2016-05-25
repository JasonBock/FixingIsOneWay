using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Immutable;

namespace FixingIsOneWay.Tests
{
	public sealed class IsOneWayOperationMakeIsOneWayFalseCodeFixTests
	{
		[Fact]
		public void GetFixableDiagnosticIds()
		{
			var fix = new IsOneWayOperationMakeIsOneWayFalseCodeFix();
			var diagnosticIds = fix.FixableDiagnosticIds;

			Assert.Equal(1, diagnosticIds.Length);
			Assert.Equal(IsOneWayOperationConstants.Id, diagnosticIds[0]);
		}

		[Fact]
		public async Task GetFixes()
		{
			var code =
@"using System.ServiceModel;

public class AClass
{
	[OperationContract(IsOneWay = true)]
	public string AMethod() { return string.Empty; }
}";

			var document = TestHelpers.Create(code);
			var tree = await document.GetSyntaxTreeAsync();
			var diagnostics = await TestHelpers.GetDiagnosticsAsync(
				code, new IsOneWayOperationAnalyzer());
			var sourceSpan = diagnostics[0].Location.SourceSpan;

			var actions = new List<CodeAction>();
			var codeActionRegistration = new Action<CodeAction, ImmutableArray<Diagnostic>>(
			  (a, _) => { actions.Add(a); });

			var fix = new IsOneWayOperationMakeIsOneWayFalseCodeFix();
			var codeFixContext = new CodeFixContext(document, diagnostics[0],
			  codeActionRegistration, new CancellationToken(false));
			await fix.RegisterCodeFixesAsync(codeFixContext);

			Assert.Equal(1, actions.Count);

			await TestHelpers.VerifyActionAsync(actions,
			  IsOneWayOperationMakeIsOneWayFalseCodeFixConstants.Description, document,
			  tree, new[] { "fals" });
		}
	}
}

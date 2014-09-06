using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FixingIsOneWay.Test
{
	[TestClass]
	public sealed class IsOneWayOperationMakeIsOneWayFalseCodeFixTests
	{
		[TestMethod]
		public void VerifyGetFixableDiagnosticIds()
		{
			var fix = new IsOneWayOperationMakeIsOneWayFalseCodeFix();
			var ids = fix.GetFixableDiagnosticIds().ToList();

			Assert.AreEqual(1, ids.Count);
			Assert.AreEqual(IsOneWayOperationConstants.DiagnosticId, ids[0]);
		}

		[TestMethod]
		public async Task VerifyGetFixes()
		{
			var code = @"
using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = true)]
	public string MyOperation() { return null; }
}";
			var document = TestHelpers.CreateDocument(code);
			var tree = await document.GetSyntaxTreeAsync();
			var diagnostics = await TestHelpers.GetDiagnosticsAsync(
				code, document, new TextSpan(119, 12));
			var sourceSpan = diagnostics[0].Location.SourceSpan;

         var fix = new IsOneWayOperationMakeIsOneWayFalseCodeFix();
			var actions = (await fix.GetFixesAsync(document, sourceSpan, diagnostics,
				new CancellationToken(false))).ToList();

			Assert.AreEqual(1, actions.Count);
			var action = actions[0];
			Assert.AreEqual(IsOneWayOperationMakeIsOneWayFalseCodeFixConstants.Description, 
				action.Description);

			var operation = (await action.GetOperationsAsync(
				new CancellationToken(false))).ToArray()[0] as ApplyChangesOperation;
			var newDoc = operation.ChangedSolution.GetDocument(document.Id);
			var newTree = await newDoc.GetSyntaxTreeAsync();
			var changes = newTree.GetChanges(tree);

			Assert.AreEqual(1, changes.Count);
			Assert.AreEqual("fals", changes[0].NewText);
		}
	}
}

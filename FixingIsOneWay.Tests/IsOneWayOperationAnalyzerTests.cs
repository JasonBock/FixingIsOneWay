using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace FixingIsOneWay.Test
{
	[TestClass]
	public sealed class IsOneWayOperationAnalyzerTests
	{
		[TestMethod]
		public void VerifySyntaxKindsOfInterest()
		{
			var analyzer = new IsOneWayOperationAnalyzer();
			var syntaxKinds = analyzer.SyntaxKindsOfInterest;
			Assert.AreEqual(1, syntaxKinds.Length);
			Assert.AreEqual(SyntaxKind.MethodDeclaration, syntaxKinds[0]);
		}

		[TestMethod]
		public void VerifySupportedDiagnostics()
		{
			var analyzer = new IsOneWayOperationAnalyzer();
			var diagnostics = analyzer.SupportedDiagnostics;
			Assert.AreEqual(1, diagnostics.Length);

			var diagnostic = diagnostics[0];
			Assert.AreEqual(diagnostic.Id, IsOneWayOperationConstants.DiagnosticId);
			Assert.AreEqual(diagnostic.Description, IsOneWayOperationConstants.Description);
			Assert.AreEqual(diagnostic.MessageFormat, IsOneWayOperationConstants.Message);
			Assert.AreEqual(diagnostic.Category, "Usage");
			Assert.AreEqual(diagnostic.DefaultSeverity, DiagnosticSeverity.Error);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayFalseAndReturnTypeIsNotVoid()
		{
			var code = @"
using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = false)]
	public string MyOperation() { return null; }
}";

			var diagnostics = await TestHelpers.GetDiagnosticsAsync(
				code, new TextSpan(120, 12));
			Assert.AreEqual(0, diagnostics.Count);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayFalseAndReturnTypeIsVoid()
		{
			var code = @"
using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = false)]
	public void MyOperation() { }
}";

			var diagnostics = await TestHelpers.GetDiagnosticsAsync(
				code, new TextSpan(118, 12));
			Assert.AreEqual(0, diagnostics.Count);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayNotSpecified()
		{
			var code = @"
using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract()]
	public string MyOperation() { return null; }
}";

			var diagnostics = await TestHelpers.GetDiagnosticsAsync(
				code, new TextSpan(104, 12));
			Assert.AreEqual(0, diagnostics.Count);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayTrueAndReturnTypeIsVoid()
		{
			var code = @"
using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = true)]
	public void MyOperation() { }
}";

			var diagnostics = await TestHelpers.GetDiagnosticsAsync(
				code, new TextSpan(117, 12));
			Assert.AreEqual(0, diagnostics.Count);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayTrueAndReturnTypeIsNotVoid()
		{
			var code = @"
using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = true)]
	public string MyOperation() { return null; }
}";

			var diagnostics = await TestHelpers.GetDiagnosticsAsync(
				code, new TextSpan(119, 12));
			Assert.AreEqual(1, diagnostics.Count);
		}
	}
}

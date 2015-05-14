using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace FixingIsOneWay.Tests
{
	[TestClass]
	public sealed class IsOneWayOperationAnalyzerTests
	{
		[TestMethod]
		public void VerifySupportedDiagnostics()
		{
			var analyzer = new IsOneWayOperationAnalyzer();
			var diagnostics = analyzer.SupportedDiagnostics;
			Assert.AreEqual(1, diagnostics.Length);

			var diagnostic = diagnostics[0];
			Assert.AreEqual(diagnostic.Id, IsOneWayOperationConstants.DiagnosticId, nameof(DiagnosticDescriptor.Id));
			Assert.AreEqual(diagnostic.Title.ToString(), IsOneWayOperationConstants.Title, nameof(DiagnosticDescriptor.Title));
			Assert.AreEqual(diagnostic.MessageFormat.ToString(), IsOneWayOperationConstants.Message, nameof(DiagnosticDescriptor.MessageFormat));
			Assert.AreEqual(diagnostic.Category, IsOneWayOperationConstants.Category, nameof(DiagnosticDescriptor.Category));
			Assert.AreEqual(diagnostic.DefaultSeverity, DiagnosticSeverity.Error, nameof(DiagnosticDescriptor.DefaultSeverity));
		}

		private static async Task RunAnalysis(string path, TextSpan span, int expectedDiagnosticCount)
		{
			var code = File.ReadAllText(path);
			var diagnostics = await TestHelpers.GetDiagnosticsAsync(code, span);
			Assert.AreEqual(expectedDiagnosticCount, diagnostics.Count, nameof(diagnostics.Count));
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayFalseAndReturnTypeIsNotVoid()
		{
			await IsOneWayOperationAnalyzerTests.RunAnalysis(
				@"Targets\IsOneWayFalseAndReturnTypeIsNotVoid.cs", new TextSpan(120, 12), 0);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayFalseAndReturnTypeIsVoid()
		{
			await IsOneWayOperationAnalyzerTests.RunAnalysis(
				@"Targets\IsOneWayFalseAndReturnTypeIsVoid.cs", new TextSpan(118, 12), 0);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayNotSpecified()
		{
			await IsOneWayOperationAnalyzerTests.RunAnalysis(
				@"Targets\IsOneWayNotSpecified.cs", new TextSpan(104, 12), 0);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayTrueAndReturnTypeIsVoid()
		{
			await IsOneWayOperationAnalyzerTests.RunAnalysis(
				@"Targets\IsOneWayTrueAndReturnTypeIsVoid.cs", new TextSpan(117, 12), 0);
		}

		[TestMethod]
		public async Task AnalyzeWithIsOneWayTrueAndReturnTypeIsNotVoid()
		{
			await IsOneWayOperationAnalyzerTests.RunAnalysis(
				@"Targets\IsOneWayTrueAndReturnTypeIsNotVoid.cs", new TextSpan(119, 12), 1);
		}
	}
}

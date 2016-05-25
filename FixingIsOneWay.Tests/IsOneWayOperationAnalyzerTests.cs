using Xunit;
using System.Threading.Tasks;

namespace FixingIsOneWay.Tests
{
	public sealed class IsOneWayOperationAnalyzerTests
	{
		[Fact]
		public void GetSupportedDiagnostics()
		{
			var analyzer = new IsOneWayOperationAnalyzer();
			var supportedDiagnostics = analyzer.SupportedDiagnostics;

			Assert.Equal(1, supportedDiagnostics.Length);

			var supportedDiagnostic = supportedDiagnostics[0];

			Assert.Equal(IsOneWayOperationConstants.Id, supportedDiagnostic.Id);
			Assert.Equal(IsOneWayOperationConstants.Title, supportedDiagnostic.Title);
			Assert.Equal(IsOneWayOperationConstants.Message, supportedDiagnostic.MessageFormat);
			Assert.Equal(IsOneWayOperationConstants.Category, supportedDiagnostic.Category);
			Assert.Equal(IsOneWayOperationConstants.Severity, supportedDiagnostic.DefaultSeverity);
		}

		[Fact]
		public async Task AnalzeNodeWhereMethodDoesNotHaveAnyAttributes()
		{
			var code =
@"public class AClass
{ 
	public void AMethod() { }
}";
			await TestHelpers.RunAnalysisAsync<IsOneWayOperationAnalyzer>(code,
			  new string[0]);
		}

		[Fact]
		public async Task AnalzeNodeWhereMethodHasAttributesButNotOperationContractAttribute()
		{
			var code =
@"public class AClass
{ 
	[Obsolete]
	public void AMethod() { }
}";
			await TestHelpers.RunAnalysisAsync<IsOneWayOperationAnalyzer>(code,
			  new string[0]);
		}

		[Fact]
		public async Task AnalzeNodeWhereMethodHasOperationContractAttributeButIsOneWayIsFalse()
		{
			var code =
@"using System.ServiceModel;

public class AClass
{
	[OperationContract]
	public void AMethod() { }
}";
			await TestHelpers.RunAnalysisAsync<IsOneWayOperationAnalyzer>(code,
			  new string[0]);
		}

		[Fact]
		public async Task AnalzeNodeWhereMethodHasOperationContractAttributeAndIsOneWayIsTrueButReturnTypeIsVoid()
		{
			var code =
@"using System.ServiceModel;

public class AClass
{
	[OperationContract(IsOneWay = true)]
	public void AMethod() { }
}";
			await TestHelpers.RunAnalysisAsync<IsOneWayOperationAnalyzer>(code,
			  new string[0]);
		}

		[Fact]
		public async Task AnalzeNodeWhereMethodHasOperationContractAttributeAndIsOneWayIsTrueAndReturnTypeIsNotVoid()
		{
			var code =
@"using System.ServiceModel;

public class AClass
{
	[OperationContract(IsOneWay = true)]
	public string AMethod() { return string.Empty; }
}";
			await TestHelpers.RunAnalysisAsync<IsOneWayOperationAnalyzer>(code,
			  new[] { IsOneWayOperationConstants.Id }, 
			  diagnostics =>
			  {
				  Assert.Equal(1, diagnostics.Length);
				  var diagnostic = diagnostics[0];
				  Assert.Equal(0, diagnostic.AdditionalLocations.Count);
				  Assert.Equal(IsOneWayOperationConstants.Title, diagnostic.Descriptor.Title);
				  Assert.Equal(IsOneWayOperationConstants.Id, diagnostic.Id);
				  Assert.Equal(IsOneWayOperationConstants.Severity, diagnostic.Severity);
				  var span = diagnostic.Location.SourceSpan;
				  Assert.Equal(74, span.Start);
				  Assert.Equal(89, span.End);
			  });
		}
	}
}

using Microsoft.CodeAnalysis;

namespace FixingIsOneWay
{
	public static class IsOneWayOperationConstants
	{
		public const string Category = "Usage";
		public const string Id = "MakeOneWayFalseDiagnosticId";
		public const string Message = "Find IsOneWay Operations With Return Values";
		public const string Title = "One-way WCF operations must return System.Void.";
		public const DiagnosticSeverity Severity = DiagnosticSeverity.Error;
	}

	public static class IsOneWayOperationReturnVoidCodeFixConstants
	{
		public const string Description = "Return System.Void";
	}

	public static class IsOneWayOperationMakeIsOneWayFalseCodeFixConstants
	{
		public const string Description = "Make IsOneWay = false";
	}
}

namespace FixingIsOneWay
{
	public static class IsOneWayOperationConstants
	{
		public const string DiagnosticId = "MakeOneWayFalseDiagnosticId";
		public const string Description = "Find IsOneWay Operations With Return Values";
		public const string IdentifierText = "IsOneWay";
		public const string Message = "One-way WCF operations must return System.Void.";
	}

	public static class IsOneWayOperationMakeIsOneWayFalseCodeFixConstants
	{
		public const string Description = "Make IsOneWay = false";
	}
}
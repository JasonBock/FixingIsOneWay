namespace FixingIsOneWay
{
	public static class IsOneWayOperationConstants
	{
		public const string Category = "Usage";
		public const string DiagnosticId = "MakeOneWayFalseDiagnosticId";
		public const string Title = "Find IsOneWay Operations With Return Values";
		public const string IdentifierText = "IsOneWay";
		public const string OperationContractTypeAssemblyName = "System.ServiceModel";
		public const string OperationContractTypeName = "OperationContractAttribute";
		public const string Message = "One-way WCF operations must return System.Void.";
	}

	public static class IsOneWayOperationMakeIsOneWayFalseCodeFixConstants
	{
		public const string Description = "Make IsOneWay = false";
	}
}

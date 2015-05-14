using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract()]
	public string MyOperation() { return null; }
}
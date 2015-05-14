using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = true)]
	public string MyOperation() { return null; }
}
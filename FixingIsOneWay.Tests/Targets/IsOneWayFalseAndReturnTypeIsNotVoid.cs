using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = false)]
	public string MyOperation() { return null; }
}
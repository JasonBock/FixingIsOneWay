using System.ServiceModel;

public sealed class OneWayTest
{
	[OperationContract(IsOneWay = true)]
	public void MyOperation() { }
}
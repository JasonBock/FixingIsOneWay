using System.ServiceModel;

namespace FixingIsOneWay.Tests
{
	public class MyService
	{
		[OperationContract(IsOneWay = true)]
		public string MyOperation() { return null; }
	}
}

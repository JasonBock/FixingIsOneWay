using System.ServiceModel;

namespace FixingIsOneWay.Tests
{
    public class MyService
    {
        [OperationContract(IsOneWay = false)]
        public string MyOperation() { return null; }
    }
}

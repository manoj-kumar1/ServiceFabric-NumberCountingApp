using System.ServiceModel;
using System.Threading.Tasks;

namespace NumberCountingService
{
    [ServiceContract]
    public interface INumberCounter
    {
        [OperationContract]
        Task<long> GetCurrentNumber();
    }
}

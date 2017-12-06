using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AMISimulator
{
    [ServiceContract]
    public interface ISimulator
    {
        [OperationContract]
        List<int> AddMeasurement(int numberOfNewConsumers);
    }
}

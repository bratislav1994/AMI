using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;

namespace FTN.ServiceContracts
{
    [ServiceContract(CallbackContract = typeof(ISimulator))]
    public interface IScadaDuplexSimulator
    {
        [OperationContract]
        int Connect();

        [OperationContract]
        List<MeasurementForScada> GetNumberOfPoints(int rtuAddress);

        [OperationContract]
        List<DataForScada> GetDataFromScada(int rtuAddress);
    }
}

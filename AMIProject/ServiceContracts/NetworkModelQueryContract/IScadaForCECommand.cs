using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    public interface IScadaForCECommand
    {
        string Command(Dictionary<long, DynamicMeasurement> measurementsInAlarm);
    }
}

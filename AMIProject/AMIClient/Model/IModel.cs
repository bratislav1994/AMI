using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public interface IModel
    {
        ObservableCollection<GeographicalRegion> GetAllRegions();
        ObservableCollection<SubGeographicalRegion> GetAllSubRegions();

        ObservableCollection<Substation> GetAllSubstations();

        ObservableCollection<EnergyConsumer> GetAllAmis();

        ObservableCollection<SubGeographicalRegion> GetSomeSubregions(long regionId);

        ObservableCollection<Substation> GetSomeSubstations(long subRegionId);

        ObservableCollection<EnergyConsumer> GetSomeAmis(long substationId);
    }
}

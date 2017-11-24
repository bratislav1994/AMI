using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace AMIClient
{
    public class SubstationViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<object> geoRegions;
        private ObservableCollection<object> subGeoRegions;
        private ObservableCollection<Substation> substations;
        private object geoRegion;
        private object subGeoRegion;

        public SubstationViewModel()
        {
            Substations = new ObservableCollection<Substation>();
            geoRegions = new ObservableCollection<object>() { "All" };
            subGeoRegions = new ObservableCollection<object>() { "All" };
            GeoRegion = GeoRegions[0];
            SubGeoRegion = SubGeoRegions[0];
            geoRegions.AddRange(TestGDA.Instance.GetAllRegions());
            subGeoRegions.AddRange(TestGDA.Instance.GetAllSubRegions());
        }

        public ObservableCollection<Substation> Substations
        {
            get
            {
                return substations;
            }

            set
            {
                substations = value;
                RaisePropertyChanged("Substations");
            }
        }
        
        public object GeoRegion
        {
            get
            {
                return geoRegion;
            }

            set
            {
                geoRegion = value;
                RaisePropertyChanged("GeoRegion");
            }
        }

        public object SubGeoRegion
        {
            get
            {
                return subGeoRegion;
            }

            set
            {
                subGeoRegion = value;
                RaisePropertyChanged("SubGeoRegion");
            }
        }

        public ObservableCollection<object> GeoRegions
        {
            get
            {
                return geoRegions;
            }

            set
            {
                geoRegions = value;
                RaisePropertyChanged("GeoRegions");
            }
        }

        public ObservableCollection<object> SubGeoRegions
        {
            get
            {
                return subGeoRegions;
            }

            set
            {
                subGeoRegions = value;
                RaisePropertyChanged("SubGeoRegions");
            }
        }

        private DelegateCommand getElementsCommand;
        public DelegateCommand GetElementsCommand
        {
            get
            {
                if (getElementsCommand == null)
                {
                    getElementsCommand = new DelegateCommand(GetElementsCommandAction);
                }

                return getElementsCommand;
            }
        }

        private void GetElementsCommandAction()
        {
            substations.Clear();

            if ((geoRegion.Equals("All") && !subGeoRegion.Equals("All")) || (!geoRegion.Equals("All") && !subGeoRegion.Equals("All")))
            {
                substations.AddRange(TestGDA.Instance.GetSomeSubstations(((Substation)SubGeoRegion).GlobalId));
            }
            else if(!geoRegion.Equals("All") && subGeoRegion.Equals("All"))
            {
                foreach(object o in subGeoRegions)
                {
                    substations.AddRange(TestGDA.Instance.GetSomeSubstations(((Substation)o).GlobalId));
                }
            }
            else
            {
                substations.AddRange(TestGDA.Instance.GetAllSubstations());
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                if (propName.Equals("GeoRegion"))
                {
                    subGeoRegions = new ObservableCollection<object>() { subGeoRegions.First() };
                    subGeoRegions.AddRange(TestGDA.Instance.GetSomeSubregions(((GeographicalRegion)geoRegion).GlobalId));
                }
            }
        }
    }
}

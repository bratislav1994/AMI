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
            GeoRegions = new ObservableCollection<object>() { "All" };
            SubGeoRegions = new ObservableCollection<object>() { "All" };
            GeoRegion = GeoRegions[0];
            SubGeoRegion = SubGeoRegions[0];
            GeoRegions.AddRange(Model.Instance.GetAllRegions());
            SubGeoRegions.AddRange(Model.Instance.GetAllSubRegions());
            Substations.AddRange(Model.Instance.GetAllSubstations());
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
            Substations.Clear();

            if ((geoRegion.Equals("All") && !subGeoRegion.Equals("All")) || (!geoRegion.Equals("All") && !subGeoRegion.Equals("All")))
            {
                substations.AddRange(Model.Instance.GetSomeSubstations(((SubGeographicalRegion)SubGeoRegion).GlobalId));
            }
            else if(!geoRegion.Equals("All") && subGeoRegion.Equals("All"))
            {
                for(int i = 1; i < SubGeoRegions.Count; i++)
                {
                    substations.AddRange(Model.Instance.GetSomeSubstations(((SubGeographicalRegion)SubGeoRegions[i]).GlobalId));
                }
            }
            else
            {
                substations.AddRange(Model.Instance.GetAllSubstations());
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
                    SubGeoRegions = new ObservableCollection<object>() { subGeoRegions.First() };
                    SubGeoRegion = subGeoRegions[0];

                    if (GeoRegion.Equals("All"))
                    {
                        SubGeoRegions.AddRange(Model.Instance.GetAllSubRegions());
                    }
                    else
                    {
                        SubGeoRegions.AddRange(Model.Instance.GetSomeSubregions(((GeographicalRegion)geoRegion).GlobalId));
                    }
                }
            }
        }
    }
}

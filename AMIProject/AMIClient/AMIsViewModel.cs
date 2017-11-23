using FTN.Common;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class AMIsViewModel : INotifyPropertyChanged
    {
        private Substation substation;
        private SubGeographicalRegion subGeoRegion;
        private GeographicalRegion geoRegion;
        private ObservableCollection<EnergyConsumer> amis;
        private ObservableCollection<GeographicalRegion> geoRegions;
        private ObservableCollection<SubGeographicalRegion> subGeoRegions;

        public Substation Substation
        {
            get
            {
                return substation;
            }

            set
            {
                substation = value;
                RaisePropertyChanged("Substation");
            }
        }

        public SubGeographicalRegion SubGeoRegion
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

        public GeographicalRegion GeoRegion
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

        public ObservableCollection<EnergyConsumer> Amis
        {
            get
            {
                return amis;
            }

            set
            {
                amis = value;
                RaisePropertyChanged("Amis");
            }
        }

        public ObservableCollection<GeographicalRegion> GeoRegions
        {
            get
            {
                return geoRegions;
            }

            set
            {
                geoRegions = value;
            }
        }

        public ObservableCollection<SubGeographicalRegion> SubGeoRegions
        {
            get
            {
                return subGeoRegions;
            }

            set
            {
                subGeoRegions = value;
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
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}

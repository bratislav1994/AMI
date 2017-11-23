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
    public class GeoSubRegionViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SubGeographicalRegion> subRegions;
        private ObservableCollection<GeographicalRegion> geoRegions;
        private GeographicalRegion geoRegion;

        public GeoSubRegionViewModel()
        {
            subRegions = new ObservableCollection<SubGeographicalRegion>();
            geoRegions = new ObservableCollection<GeographicalRegion>();
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
                RaisePropertyChanged("GeoRegion");
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
            }
        }

        public ObservableCollection<SubGeographicalRegion> SubRegions
        {
            get
            {
                return subRegions;
            }

            set
            {
                subRegions = value;
                RaisePropertyChanged("SubRegions");
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

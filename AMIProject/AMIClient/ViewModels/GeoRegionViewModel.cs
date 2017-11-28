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

namespace AMIClient
{
    public class GeoRegionViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<GeographicalRegion> geoRegions;
        private IModel model;

        public GeoRegionViewModel(IModel model)
        {
            this.model = model;
            geoRegions = new ObservableCollection<GeographicalRegion>();
            GeoRegions.AddRange(this.model.GetAllRegions());
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
                RaisePropertyChanged("GeoRegions");
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
            set
            {
                getElementsCommand = value;
            }
        }


        private void GetElementsCommandAction()
        {
            geoRegions.Clear();
            geoRegions.AddRange(this.model.GetAllRegions());
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

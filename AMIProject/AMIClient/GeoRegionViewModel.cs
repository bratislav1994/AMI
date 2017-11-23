using AMIClient.ClassesForTable;
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
        private ObservableCollection<GeoRegionForTable> geoRegions;

        public GeoRegionViewModel()
        {
            geoRegions = new ObservableCollection<GeoRegionForTable>();
        }
        
        public ObservableCollection<GeoRegionForTable> GeoRegions
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
        }


        private void GetElementsCommandAction()
        {
            TestGDA tgda = new TestGDA();
            GeoRegions = tgda.GetExtentValues(ModelCode.GEOREGION);
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

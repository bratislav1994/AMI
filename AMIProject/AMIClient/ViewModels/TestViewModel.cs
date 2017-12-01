using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class TestViewModel : INotifyPropertyChanged
    {
        IModel model;
        private ObservableCollection<GeoRegionForTree> geoRegions;
        private ObservableCollection<EnergyConsumer> amis = new ObservableCollection<EnergyConsumer>();
        private DateTime newChange;
        private DateTime oldCahnge;
        Thread CheckLists;
        private object lockObject;

        public TestViewModel(IModel model)
        {
            this.model = model;
            ObservableCollection<GeographicalRegion> temp = this.model.GetAllRegions();
            geoRegions = new ObservableCollection<GeoRegionForTree>();
            lockObject = new object();
            foreach (GeographicalRegion gr in temp)
            {
                GeoRegions.Add(new GeoRegionForTree(gr, model, ref amis, ref newChange));
            }
            newChange = DateTime.Now;
            oldCahnge = DateTime.Now;
            CheckLists = new Thread(() => ThreadFunction());
            CheckLists.IsBackground = true;
            CheckLists.Start();

        }

        public ObservableCollection<GeoRegionForTree> GeoRegions
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


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private void ThreadFunction()
        {
            while(true)
            {
                if(this.newChange > this.oldCahnge)
                {
                    RaisePropertyChanged("Amis");
                    this.oldCahnge = this.newChange;
                }
                Thread.Sleep(200);
            }
        }
        
        public void AbortThread(object sender, CancelEventArgs e)
        {
            this.CheckLists.Abort();
        }
    }
}

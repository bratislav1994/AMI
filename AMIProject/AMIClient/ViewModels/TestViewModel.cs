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
        private ObservableCollection<EnergyConsumer> oldAmis = new ObservableCollection<EnergyConsumer>();
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
                GeoRegions.Add(new GeoRegionForTree(gr, model, ref amis, ref lockObject));
            }
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
                lock (lockObject)
                {
                    if (!CompareLists(amis, oldAmis))
                    {
                        oldAmis.Clear();
                        oldAmis.AddRange(amis);
                        RaisePropertyChanged("Amis");
                    }
                }
                Thread.Sleep(200);
            }
        }

        private bool CompareLists(ObservableCollection<EnergyConsumer> current, ObservableCollection<EnergyConsumer> old)
        {
            if(current.Count != old.Count)
            {
                return false;
            }
            for(int i=0; i<current.Count;i++)
            {
                if(current[i].GlobalId != old[i].GlobalId)
                {
                    return false;
                }
            }

            return true;
        }

        public void AbortThread(object sender, CancelEventArgs e)
        {
            this.CheckLists.Abort();
        }
    }
}

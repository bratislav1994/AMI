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

namespace AMIClient.ViewModels
{
    public class NetworkPreviewViewModel : INotifyPropertyChanged
    {
        public IModel model;
        private ObservableCollection<RootElement> rootElements;
        private ObservableCollection<EnergyConsumer> amis = new ObservableCollection<EnergyConsumer>();
        private DateTime newChange;
        private DateTime oldCahnge;
        Thread CheckLists;
        private static NetworkPreviewViewModel instance;

        public NetworkPreviewViewModel()
        {
            rootElements = new ObservableCollection<RootElement>();
            RootElements.Add(new RootElement(Model.Instance, ref amis, ref newChange));
            Model.Instance.SetRoot(rootElements[0]);
            newChange = DateTime.Now;
            oldCahnge = DateTime.Now;
            CheckLists = new Thread(() => ThreadFunction());
            CheckLists.IsBackground = true;
            CheckLists.Start();
        }

        public static NetworkPreviewViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NetworkPreviewViewModel();
                }

                return instance;
            }
        }

        public ObservableCollection<RootElement> RootElements
        {
            get
            {
                return rootElements;
            }

            set
            {
                rootElements = value;
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

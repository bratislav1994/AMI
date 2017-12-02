using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class RootElement : TreeClasses, IDisposable
    {
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private string name;
        private bool needsUpdate = false;
        private object lockObject;
        private Thread updateThread;

        public RootElement(IModel model, ref ObservableCollection<EnergyConsumer> amis, ref DateTime newChange)
            :base(null, model)
        {
            LockObject = new object();
            this.amis = amis;
            this.newChange = newChange;
            this.IsExpanded = false;
            this.name = "All";
            updateThread = new Thread(() => CheckForUpdates());
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }

            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                if (Children.Count == 0)
                {
                    LoadChildren();
                }
            }
        }


        public bool IsSelected
        {
            get
            {
                return base.isSelected;
            }

            set
            {
                if (value != base.isSelected)
                {
                    this.amis.Clear();
                    base.isSelected = value;
                    this.amis.AddRange(base.Model.GetAllAmis());
                    this.newChange = DateTime.Now;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public bool NeedsUpdate
        {
            get
            {
                return needsUpdate;
            }

            set
            {
                needsUpdate = value;
            }
        }

        public object LockObject
        {
            get
            {
                return lockObject;
            }

            set
            {
                lockObject = value;
            }
        }

        protected override void LoadChildren()
        {
            base.Children.Clear();
            ObservableCollection<GeographicalRegion> temp = this.Model.GetAllRegions();
            foreach (GeographicalRegion gr in temp)
            {
                base.Children.Add(new GeoRegionForTree(this, gr, this.Model, ref this.amis, ref this.newChange));
            }
        }

        private void CheckForUpdates()
        {
            while (true)
            {
                lock (LockObject)
                {
                    if (needsUpdate)
                    {
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            LoadChildren();
                        }));
                        needsUpdate = false;
                    }
                }
                Thread.Sleep(200);
            }
        }

        public void Dispose()
        {
            updateThread.Abort();
        }
    }
}
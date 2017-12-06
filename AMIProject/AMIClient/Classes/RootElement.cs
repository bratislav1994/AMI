using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class RootElement : TreeClasses, IDisposable
    {
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private string name = "All";
        private bool needsUpdate = false;
        private object lockObject = new object();
        private Thread updateThread;
        private Dictionary<long, TreeClasses> allTreeElements = new Dictionary<long, TreeClasses>();

        public RootElement(IModel model, ref ObservableCollection<EnergyConsumer> amis, ref DateTime newChange)
            :base(null, model)
        {
            this.amis = amis;
            this.newChange = newChange;
            this.IsExpanded = false;
            updateThread = new Thread(() => CheckForUpdates());
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        public RootElement() : base()
        {

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

        public override void LoadChildren()
        {
            ObservableCollection<GeographicalRegion> temp = this.Model.GetAllRegions();

            foreach (GeographicalRegion gr in temp)
            {
                if (!allTreeElements.ContainsKey(gr.GlobalId))
                {
                    base.Children.Add(new GeoRegionForTree(this, gr, this.Model, ref this.amis, ref this.newChange, ref allTreeElements));
                    allTreeElements.Add(gr.GlobalId, base.Children[base.Children.Count - 1]);
                }
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
                            if(IsSelected)
                            {
                                this.amis.Clear();
                                this.amis.AddRange(this.Model.GetAllAmis());
                                this.newChange = DateTime.Now;
                            }
                            else
                            {
                                foreach(TreeClasses tc in allTreeElements.Values)
                                {
                                    tc.CheckIfSeleacted();
                                }
                            }
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
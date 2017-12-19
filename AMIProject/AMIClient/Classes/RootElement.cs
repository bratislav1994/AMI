using FTN.Common.Logger;
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
        private string name = "All";
        private bool needsUpdate = false;
        private object lockObject = new object();
        private Thread updateThread;
        private Dictionary<long, TreeClasses> allTreeElements = new Dictionary<long, TreeClasses>();
        private Dispatcher dispatcher;

        public RootElement(Model model)
            :base(null, model)
        {
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
                    base.isSelected = value;
                    base.Model.ClearPositions();
                    Model.GetAllAmis();
                    
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

        public Dictionary<long, TreeClasses> AllTreeElements
        {
            get
            {
                return allTreeElements;
            }

            set
            {
                allTreeElements = value;
            }
        }

        public override void LoadChildren()
        {
            Logger.LogMessageToFile(string.Format("AMIClient.RootElement.LoadChildren; line: {0}; Start the LoadChildren function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()), "Client.txt");
            this.Model.GeoRegions.Clear();
            this.Model.GetAllRegions();

            if (this.Model.GeoRegions != null)
            {
                foreach (GeographicalRegion gr in this.Model.GeoRegions)
                {
                    if (!AllTreeElements.ContainsKey(gr.GlobalId))
                    {
                        base.Children.Add(new GeoRegionForTree(this, gr, this.Model, ref allTreeElements));
                        AllTreeElements.Add(gr.GlobalId, base.Children[base.Children.Count - 1]);
                    }
                }
            }

            Logger.LogMessageToFile(string.Format("AMIClient.RootElement.LoadChildren; line: {0}; Finish the LoadChildren function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()), "Client.txt");
        }
        
        //public Dispatcher Dispatcher
        //{
        //    get
        //    {
        //        return this.dispatcher;
        //    }

        //    set
        //    {
        //        this.dispatcher = value;
        //    }
        //}

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
                            base.Model.ClearAmis();
                            if (IsSelected)
                            {
                                this.Model.GetAllAmis();
                            }
                            else
                            {
                                foreach(TreeClasses tc in AllTreeElements.Values)
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
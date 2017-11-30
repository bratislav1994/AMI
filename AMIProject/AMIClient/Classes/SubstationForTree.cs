using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class SubstationForTree : TreeClasses
    {
        private Substation substation;
        private ObservableCollection<EnergyConsumer> amis;
        private object lockObject;

        public SubstationForTree(Substation substation, SubGeoRegionForTree parent, IModel model, ref ObservableCollection<EnergyConsumer> amis, ref object lockObject)
            : base(parent, model)
        {
            this.substation = substation;
            this.amis = amis;
            this.lockObject = lockObject;
            this.IsExpanded = false;
        }

        public string Name
        {
            get
            {
                return this.substation.Name;
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

                if (isExpanded && base.Parent != null)
                {
                    ((SubGeoRegionForTree)base.Parent).IsExpanded = true;
                }

                base.Children.Clear();
                LoadChildren();
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
                    this.amis.Clear();
                    lock (lockObject)
                    {
                        this.amis.AddRange(base.Model.GetSomeAmis(this.substation.GlobalId));
                    }
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }
    }
}

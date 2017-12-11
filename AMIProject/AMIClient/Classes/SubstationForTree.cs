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
        private DateTime newChange;

        public SubstationForTree(Substation substation, SubGeoRegionForTree parent, IModel model, ref ObservableCollection<EnergyConsumer> amis, ref DateTime newChange)
            : base(parent, model)
        {
            this.Substation = substation;
            this.amis = amis;
            this.newChange = newChange;
            this.IsExpanded = false;
        }

        public SubstationForTree() : base()
        {

        }

        public string Name
        {
            get
            {
                return this.Substation.Name;
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
                    this.amis.AddRange(base.Model.GetSomeAmis(this.Substation.GlobalId));
                    this.newChange = DateTime.Now;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public Substation Substation
        {
            get
            {
                return substation;
            }

            set
            {
                substation = value;
            }
        }

        public override void CheckIfSeleacted()
        {
            if (IsSelected)
            {
                this.amis.Clear();
                this.amis.AddRange(base.Model.GetSomeAmis(this.Substation.GlobalId));
                this.newChange = DateTime.Now;
            }
        }
    }
}

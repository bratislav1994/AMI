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
    public class SubGeoRegionForTree : TreeClasses
    {
        private SubGeographicalRegion subGeoRegion;
        private ObservableCollection<EnergyConsumer> amis;
        private object lockObject;

        public SubGeoRegionForTree(SubGeographicalRegion subGeoRegion, GeoRegionForTree parent, IModel model, ref ObservableCollection<EnergyConsumer> amis, ref object lockObject)
            : base(parent, model)
        {
            this.subGeoRegion = subGeoRegion;
            this.amis = amis;
            this.lockObject = lockObject;
            this.IsExpanded = false;
        }

        public string Name
        {
            get
            {
                return this.subGeoRegion.Name;
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
                    ((GeoRegionForTree)base.Parent).IsExpanded = true;
                }

                if (value == false)
                {
                    base.Children.Clear();
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
                    ObservableCollection<Substation> ssTemp = new ObservableCollection<Substation>();
                    ssTemp.AddRange(base.Model.GetSomeSubstations(this.subGeoRegion.GlobalId));
                    lock (lockObject)
                    {
                        this.amis.Clear();
                        foreach (Substation ss in ssTemp)
                        {
                            this.amis.AddRange(base.Model.GetSomeAmis(ss.GlobalId));
                        }
                    }
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        protected override void LoadChildren()
        {
            ObservableCollection<Substation> temp = base.Model.GetSomeSubstations(this.subGeoRegion.GlobalId);
            foreach(Substation ss in temp)
            {
                base.Children.Add(new SubstationForTree(ss, this, this.Model, ref this.amis, ref this.lockObject));
            }
        }
    }
}

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
        private DateTime newChange;
        private Dictionary<long, TreeClasses> allTreeElements;

        public SubGeoRegionForTree(SubGeographicalRegion subGeoRegion, GeoRegionForTree parent, IModel model, ref ObservableCollection<EnergyConsumer> amis, ref DateTime newChange, ref Dictionary<long, TreeClasses> allTreeElements)
            : base(parent, model)
        {
            this.allTreeElements = allTreeElements;
            this.SubGeoRegion = subGeoRegion;
            this.amis = amis;
            this.newChange = newChange;
            this.IsExpanded = false;
        }

        public string Name
        {
            get
            {
                return this.SubGeoRegion.Name;
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
                    ssTemp.AddRange(base.Model.GetSomeSubstations(this.SubGeoRegion.GlobalId));
                    this.amis.Clear();
                    foreach (Substation ss in ssTemp)
                    {
                        this.amis.AddRange(base.Model.GetSomeAmis(ss.GlobalId));
                    }
                    this.newChange = DateTime.Now;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public SubGeographicalRegion SubGeoRegion
        {
            get
            {
                return subGeoRegion;
            }

            set
            {
                subGeoRegion = value;
            }
        }

        protected override void LoadChildren()
        {
            ObservableCollection<Substation> temp = base.Model.GetSomeSubstations(this.SubGeoRegion.GlobalId);
            foreach(Substation ss in temp)
            {
                if (!allTreeElements.ContainsKey(ss.GlobalId))
                {
                    base.Children.Add(new SubstationForTree(ss, this, this.Model, ref this.amis, ref this.newChange));
                    allTreeElements.Add(ss.GlobalId, base.Children[base.Children.Count - 1]);
                }
            }
        }

        public override void CheckIfSeleacted()
        {
            if(IsSelected)
            {
                ObservableCollection<Substation> ssTemp = new ObservableCollection<Substation>();
                ssTemp.AddRange(base.Model.GetSomeSubstations(this.SubGeoRegion.GlobalId));
                this.amis.Clear();
                foreach (Substation ss in ssTemp)
                {
                    this.amis.AddRange(base.Model.GetSomeAmis(ss.GlobalId));
                }
                this.newChange = DateTime.Now;
            }
        }
    }
}

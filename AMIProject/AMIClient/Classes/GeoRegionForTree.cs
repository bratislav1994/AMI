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
    public class GeoRegionForTree : TreeClasses
    {
        private GeographicalRegion geoRegion;
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private Dictionary<long, TreeClasses> allTreeElements;

        public GeoRegionForTree(TreeClasses parent, GeographicalRegion geoRegion, IModel model, ref ObservableCollection<EnergyConsumer> amis, ref DateTime newChange, ref Dictionary<long, TreeClasses> allTreeElements)
            :base(parent, model)
        {
            this.GeoRegion = geoRegion;
            this.Amis = amis;
            this.newChange = newChange;
            this.allTreeElements = allTreeElements;
            this.IsExpanded = false;
        }

        public GeoRegionForTree() : base()
        {

        }

        public string Name
        {
            get
            {
                return this.GeoRegion.Name;
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
                
                if(Children.Count == 0)
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
                    ObservableCollection<SubGeographicalRegion> sgrTemp = base.Model.GetSomeSubregions(this.GeoRegion.GlobalId);
                    ObservableCollection<Substation> ssTemp = new ObservableCollection<Substation>();
                    foreach(SubGeographicalRegion sgr in sgrTemp)
                    {
                        ssTemp.AddRange(base.Model.GetSomeSubstations(sgr.GlobalId));
                    }
                    this.Amis.Clear();
                    foreach (Substation ss in ssTemp)
                    {
                        this.Amis.AddRange(base.Model.GetSomeAmis(ss.GlobalId));
                    }
                    this.newChange = DateTime.Now;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public GeographicalRegion GeoRegion
        {
            get
            {
                return geoRegion;
            }

            set
            {
                geoRegion = value;
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
            }
        }

        public override void LoadChildren()
        {
            ObservableCollection<SubGeographicalRegion> temp = this.Model.GetSomeSubregions(GeoRegion.GlobalId);

            if (temp != null)
            {
                foreach (SubGeographicalRegion sgr in temp)
                {
                    if (!allTreeElements.ContainsKey(sgr.GlobalId))
                    {
                        base.Children.Add(new SubGeoRegionForTree(sgr, this, this.Model, ref this.amis, ref this.newChange, ref allTreeElements));
                        allTreeElements.Add(sgr.GlobalId, base.Children[base.Children.Count - 1]);
                    }
                }
            }
        }

        public override void CheckIfSeleacted()
        {
            if (IsSelected)
            {
                ObservableCollection<SubGeographicalRegion> sgrTemp = base.Model.GetSomeSubregions(this.GeoRegion.GlobalId);
                ObservableCollection<Substation> ssTemp = new ObservableCollection<Substation>();
                foreach (SubGeographicalRegion sgr in sgrTemp)
                {
                    ssTemp.AddRange(base.Model.GetSomeSubstations(sgr.GlobalId));
                }
                this.Amis.Clear();
                foreach (Substation ss in ssTemp)
                {
                    this.Amis.AddRange(base.Model.GetSomeAmis(ss.GlobalId));
                }
                this.newChange = DateTime.Now;
            }
        }
    }
}

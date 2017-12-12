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
        private Dictionary<long, TreeClasses> allTreeElements;

        public GeoRegionForTree(TreeClasses parent, GeographicalRegion geoRegion, Model model, ref Dictionary<long, TreeClasses> allTreeElements)
            :base(parent, model)
        {
            this.GeoRegion = geoRegion;
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
                    base.Model.SubGeoRegions.Clear();
                    base.Model.GetSomeSubregions(this.GeoRegion.GlobalId);
                    base.Model.Substations.Clear();
                    foreach(SubGeographicalRegion sgr in base.Model.SubGeoRegions)
                    {
                        base.Model.GetSomeSubstations(sgr.GlobalId);
                    }
                    base.Model.Amis.Clear();
                    foreach (Substation ss in base.Model.Substations)
                    {
                        base.Model.GetSomeAmis(ss.GlobalId);
                    }
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

        public override void LoadChildren()
        {
            this.Model.SubGeoRegions.Clear();
            this.Model.GetSomeSubregions(GeoRegion.GlobalId);

            if (this.Model.SubGeoRegions != null)
            {
                foreach (SubGeographicalRegion sgr in this.Model.SubGeoRegions)
                {
                    if (!allTreeElements.ContainsKey(sgr.GlobalId))
                    {
                        base.Children.Add(new SubGeoRegionForTree(sgr, this, this.Model, ref allTreeElements));
                        allTreeElements.Add(sgr.GlobalId, base.Children[base.Children.Count - 1]);
                    }
                }
            }
        }

        public override void CheckIfSeleacted()
        {
            if (IsSelected)
            {
                base.Model.GetSomeSubregions(this.GeoRegion.GlobalId);
                base.Model.Substations.Clear();
                foreach (SubGeographicalRegion sgr in base.Model.SubGeoRegions)
                {
                    base.Model.GetSomeSubstations(sgr.GlobalId);
                }
                base.Model.Amis.Clear();
                foreach (Substation ss in base.Model.Substations)
                {
                    base.Model.GetSomeAmis(ss.GlobalId);
                }
            }
        }
    }
}

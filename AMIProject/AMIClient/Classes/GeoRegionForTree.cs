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
        private object lockObject;

        public GeoRegionForTree(GeographicalRegion geoRegion, IModel model, ref ObservableCollection<EnergyConsumer> amis, ref object lockObject)
            :base(null, model)
        {
            this.geoRegion = geoRegion;
            this.amis = amis;
            this.lockObject = lockObject;
            this.IsExpanded = false;
        }

        public string Name
        {
            get
            {
                return this.geoRegion.Name;
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

                if(value == false)
                {
                    Children.Clear();
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
                    ObservableCollection<SubGeographicalRegion> sgrTemp = base.Model.GetSomeSubregions(this.geoRegion.GlobalId);
                    ObservableCollection<Substation> ssTemp = new ObservableCollection<Substation>();
                    foreach(SubGeographicalRegion sgr in sgrTemp)
                    {
                        ssTemp.AddRange(base.Model.GetSomeSubstations(sgr.GlobalId));
                    }
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
            ObservableCollection<SubGeographicalRegion> temp = this.Model.GetSomeSubregions(geoRegion.GlobalId);
            foreach(SubGeographicalRegion sgr in temp)
            {
                base.Children.Add(new SubGeoRegionForTree(sgr, this, this.Model, ref this.amis, ref this.lockObject));
            }
        }
    }
}

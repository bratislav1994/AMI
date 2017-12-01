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
    public class RootElement : TreeClasses
    {
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private string name;

        public RootElement(IModel model, ref ObservableCollection<EnergyConsumer> amis, ref DateTime newChange)
            :base(null, model)
        {
            this.amis = amis;
            this.newChange = newChange;
            this.IsExpanded = false;
            this.name = "All";
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

                if (value == false)
                {
                    Children.Clear();
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

        protected override void LoadChildren()
        {
            ObservableCollection<GeographicalRegion> temp = this.Model.GetAllRegions();
            foreach (GeographicalRegion gr in temp)
            {
                base.Children.Add(new GeoRegionForTree(this, gr, this.Model, ref this.amis, ref this.newChange));
            }
        }

    }
}
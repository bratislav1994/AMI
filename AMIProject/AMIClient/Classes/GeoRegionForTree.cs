using AMIClient.ViewModels;
using FTN.Common.Logger;
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
            : base(parent, model)
        {
            this.GeoRegion = geoRegion;
            this.AllTreeElements = allTreeElements;
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
                    if (value)
                    {
                        Logger.LogMessageToFile(string.Format("AMIClient.GeoRegionForTree.IsSelected; line: {0}; Start - get all ami for the selected region", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                        base.Model.ClearPositions();
                        base.Model.ClearTableItems();
                        base.Model.GetSomeTableItemsForGeoRegion(this.GeoRegion.GlobalId);
                        this.OnPropertyChanged("IsSelected");
                        Logger.LogMessageToFile(string.Format("AMIClient.GeoRegionForTree.IsSelected; line: {0}; Finish - get all ami for the selected region", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    }
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
            Logger.LogMessageToFile(string.Format("AMIClient.GeoRegionForTree.LoadChildren; line: {0}; Start the LoadChildren function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<IdentifiedObject> SubGeoRegions = new List<IdentifiedObject>();
            SubGeoRegions = this.Model.GetSomeSubregions(GeoRegion.GlobalId, false);

            if (SubGeoRegions != null)
            {
                foreach (SubGeographicalRegion sgr in SubGeoRegions)
                {
                    if (!AllTreeElements.ContainsKey(sgr.GlobalId))
                    {
                        base.Children.Add(new SubGeoRegionForTree(sgr, this, this.Model, ref allTreeElements));
                        AllTreeElements.Add(sgr.GlobalId, base.Children[base.Children.Count - 1]);
                    }
                }
            }

            Logger.LogMessageToFile(string.Format("AMIClient.GeoRegionForTree.LoadChildren; line: {0}; Finish the LoadChildren function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public override void CheckIfSeleacted()
        {
            if (IsSelected)
            {
                List<IdentifiedObject> SubGeoRegions = new List<IdentifiedObject>();
                List<IdentifiedObject> Substations = new List<IdentifiedObject>();
                SubGeoRegions = base.Model.GetSomeSubregions(this.GeoRegion.GlobalId, false);
                base.Model.ClearPositions();
                foreach (SubGeographicalRegion sgr in SubGeoRegions)
                {
                    Substations.AddRange(base.Model.GetSomeSubstations(sgr.GlobalId, false));
                }

                base.Model.ClearTableItems();

                foreach (Substation ss in Substations)
                {
                    base.Model.GetSomeTableItems(ss.GlobalId, false);
                }
            }
        }
    }
}

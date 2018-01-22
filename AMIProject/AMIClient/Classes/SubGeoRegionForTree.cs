﻿using AMIClient.ViewModels;
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
    public class SubGeoRegionForTree : TreeClasses
    {
        private SubGeographicalRegion subGeoRegion;
        private Dictionary<long, TreeClasses> allTreeElements;

        public SubGeoRegionForTree(SubGeographicalRegion subGeoRegion, GeoRegionForTree parent, Model model, ref Dictionary<long, TreeClasses> allTreeElements)
            : base(parent, model)
        {
            this.allTreeElements = allTreeElements;
            this.SubGeoRegion = subGeoRegion;
            this.IsExpanded = false;
        }

        public SubGeoRegionForTree() : base()
        {

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
                    if (value)
                    {
                        Logger.LogMessageToFile(string.Format("AMIClient.SubGeoRegionForTree.IsSelected; line: {0}; Start - get all ami for the selected region", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                        base.Model.ClearPositions();
                        base.Model.ClearTableItems();
                        base.Model.GetSomeTableItemsForSubGeoRegion(this.SubGeoRegion.GlobalId);
                        this.OnPropertyChanged("IsSelected");
                        Logger.LogMessageToFile(string.Format("AMIClient.SubGeoRegionForTree.IsSelected; line: {0}; Finish - get all ami for the selected region", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    }
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
            Logger.LogMessageToFile(string.Format("AMIClient.SubGeoRegionForTree.LoadChildren; line: {0}; Start the LoadChildren function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<IdentifiedObject> Substations = new List<IdentifiedObject>();
            Substations = base.Model.GetSomeSubstations(this.SubGeoRegion.GlobalId, false);

            if (Substations != null)
            {
                foreach (Substation ss in Substations)
                {
                    if (!allTreeElements.ContainsKey(ss.GlobalId))
                    {
                        base.Children.Add(new SubstationForTree(ss, this, this.Model));
                        allTreeElements.Add(ss.GlobalId, base.Children[base.Children.Count - 1]);
                    }
                }
            }

            Logger.LogMessageToFile(string.Format("AMIClient.SubGeoRegionForTree.LoadChildren; line: {0}; Finish the LoadChildren function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public override void CheckIfSeleacted()
        {
            if (IsSelected)
            {
                List<IdentifiedObject> Substations = new List<IdentifiedObject>();
                Substations = base.Model.GetSomeSubstations(this.SubGeoRegion.GlobalId, false);
                base.Model.ClearTableItems();
                base.Model.ClearPositions();
                foreach (Substation ss in Substations)
                {
                    base.Model.GetSomeTableItems(ss.GlobalId, false);
                }
            }
        }
    }
}

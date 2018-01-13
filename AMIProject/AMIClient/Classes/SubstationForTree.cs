using AMIClient.ViewModels;
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

        public SubstationForTree(Substation substation, SubGeoRegionForTree parent, Model model)
            : base(parent, model)
        {
            this.Substation = substation;
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
                    if (!NetworkPreviewViewModel.Instance.IsRightClick() && value)
                    {
                        base.Model.ClearTableItems();
                        base.Model.ClearPositions();
                        base.Model.GetSomeTableItems(this.Substation.GlobalId, false);
                        this.OnPropertyChanged("IsSelected");
                    }
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
            base.Model.ClearTableItems();
            base.Model.ClearPositions();

            if (IsSelected)
            {
                base.Model.GetSomeTableItems(this.Substation.GlobalId, false);
            }
        }
    }
}

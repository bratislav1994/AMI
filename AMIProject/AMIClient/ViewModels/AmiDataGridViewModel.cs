using AMIClient.HelperClasses;
using FTN.Common;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient.ViewModels
{
    public class AmiDataGridViewModel : AvalonDockMVVM.ViewModel.DockWindowViewModel, INotifyPropertyChanged
    {
        private Model model;
        private Dictionary<string, string> columnFilters;
        private Dictionary<string, PropertyInfo> propertyCache;
        private string nameFilter = string.Empty;
        private string typeFilter = string.Empty;
        private DMSType parentType;
        private long parentGid;
        private Dictionary<long, int> positionsAmi = new Dictionary<long, int>();
        private ICollectionView viewAmiTableItems;
        private ObservableCollection<TableItem> amiTableItems = new ObservableCollection<TableItem>();

        public AmiDataGridViewModel()
        {

        }

        public string NameFilter
        {
            get
            {
                return nameFilter;
            }

            set
            {
                nameFilter = value;
                this.RaisePropertyChanged("NameFilter");
                columnFilters[DataGridHeader.Name.ToString()] = this.nameFilter;
                this.OnFilterApply();
            }
        }

        public string TypeFilter
        {
            get
            {
                return typeFilter;
            }

            set
            {
                typeFilter = value;
                this.RaisePropertyChanged("TypeFilter");
                columnFilters[DataGridHeader.Type.ToString()] = this.typeFilter;
                this.OnFilterApply();
            }
        }

        public Model Model
        {
            get
            {
                return model;
            }

            set
            {
                model = value;
            }
        }

        public long ParentGid
        {
            get
            {
                return parentGid;
            }

            set
            {
                parentGid = value;
            }
        }

        public DMSType ParentType
        {
            get
            {
                return parentType;
            }

            set
            {
                parentType = value;
                this.GetAmisForParentType();
            }
        }

        private void GetAmisForParentType()
        {
            switch (this.ParentType)
            {
                case DMSType.GEOREGION:
                    List<IdentifiedObject> subRegionsC = this.Model.GetSomeSubregions(ParentGid, true);
                    List<IdentifiedObject> substationsC = new List<IdentifiedObject>();

                    foreach (SubGeographicalRegion sgr in subRegionsC)
                    {
                        substationsC.AddRange(this.Model.GetSomeSubstations(sgr.GlobalId, true));
                    }

                    this.Model.AmiTableItems.Clear();

                    foreach (Substation ss in substationsC)
                    {
                        this.Model.GetSomeTableItems(ss.GlobalId, false);
                    }
                    break;
                case DMSType.SUBGEOREGION:

                    break;
                case DMSType.SUBSTATION:

                    break;
            }
        }

        public void SetModel(Model model)
        {
            this.Model = model;
            this.propertyCache = new Dictionary<string, PropertyInfo>();
            columnFilters = new Dictionary<string, string>();
            columnFilters[DataGridHeader.Name.ToString()] = string.Empty;
            columnFilters[DataGridHeader.Type.ToString()] = string.Empty;
            this.Model.ViewAmiTableItems = new CollectionViewSource { Source = this.Model.AmiTableItems }.View;
            this.Model.ViewAmiTableItems = CollectionViewSource.GetDefaultView(this.Model.AmiTableItems);
        }

        #region filter

        public void OnFilterApply()
        {
            this.Model.ViewAmiTableItems = CollectionViewSource.GetDefaultView(this.Model.AmiTableItems);

            if (this.Model.ViewAmiTableItems != null)
            {
                this.Model.ViewAmiTableItems.Filter = delegate (object item)
                {
                    bool show = true;

                    foreach (KeyValuePair<string, string> filter in columnFilters)
                    {
                        bool containsFilter = false;

                        if (filter.Key.Equals(DataGridHeader.Name.ToString()))
                        {
                            containsFilter = ((TableItem)item).Io.Name.IndexOf(NameFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }
                        else if (filter.Key.Equals(DataGridHeader.Type.ToString()))
                        {
                            DataGridType type = ((TableItem)item).Type;
                            containsFilter = EnumDescription.GetEnumDescription(type).IndexOf(TypeFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }

                        if (!containsFilter)
                        {
                            show = false;
                            break;
                        }
                    }

                    return show;
                };
            }
        }

        #endregion

        private ICommand individualAmiChartCommand;

        public ICommand IndividualAmiChartCommand
        {
            get
            {
                return this.individualAmiChartCommand ?? (this.individualAmiChartCommand = new DelegateCommand<object>(this.SelectedAMIAction, param => true));
            }
        }

        private ICommand individualAmiHourChartCommand;

        public ICommand IndividualAmiHourChartCommand
        {
            get
            {
                return this.individualAmiHourChartCommand ?? (this.individualAmiHourChartCommand = new DelegateCommand<object>(this.SelectedAMIHourAction, param => true));
            }
        }

        private ICommand individualAmiDayChartCommand;

        public ICommand IndividualAmiDayChartCommand
        {
            get
            {
                return this.individualAmiDayChartCommand ?? (this.individualAmiDayChartCommand = new DelegateCommand<object>(this.SelectedAMIDayAction, param => true));
            }
        }

        public ICollectionView ViewAmiTableItems
        {
            get
            {
                return viewAmiTableItems;
            }

            set
            {
                viewAmiTableItems = value;
            }
        }

        private void SelectedAMIAction(object selected)
        {
            this.SendValues(ResolutionType.MINUTE, selected);
        }

        private void SelectedAMIHourAction(object selected)
        {
            this.SendValues(ResolutionType.HOUR, selected);
        }

        private void SelectedAMIDayAction(object selected)
        {
            this.SendValues(ResolutionType.DAY, selected);
        }

        private void SendValues(ResolutionType resolution, object selected)
        {
            IdentifiedObject io = ((IdentifiedObject)selected);
            NetworkPreviewViewModel.Instance.SelectedAMIAction(io, resolution);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}


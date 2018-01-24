﻿using AMIClient.HelperClasses;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private DateTime timeOfLastUpdate = DateTime.Now;
        private Thread checkIfThereAreNewUpdates;

        public AmiDataGridViewModel()
        {
            this.checkIfThereAreNewUpdates = new Thread(() => CheckForUpdates());
            this.checkIfThereAreNewUpdates.Start();
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
                this.GetAmisForParentType();
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

                    this.AmiTableItems.Clear();
                    List<IdentifiedObject> amis = new List<IdentifiedObject>();

                    foreach (Substation ss in substationsC)
                    {
                        amis.AddRange(this.Model.GetSomeAmis(ss.GlobalId));
                    }

                    foreach (IdentifiedObject io in amis)
                    {
                        this.AmiTableItems.Add(new TableItem(io));
                        this.positionsAmi.Add(io.GlobalId, this.AmiTableItems.Count - 1);
                    }

                    break;
                case DMSType.SUBGEOREGION:
                    List<IdentifiedObject> substationsC2 = new List<IdentifiedObject>();
                    
                    substationsC2.AddRange(this.Model.GetSomeSubstations(ParentGid, true));

                    this.AmiTableItems.Clear();
                    List<IdentifiedObject> amisC2 = new List<IdentifiedObject>();

                    foreach (Substation ss in substationsC2)
                    {
                        amisC2.AddRange(this.Model.GetSomeAmis(ss.GlobalId));
                    }

                    foreach (IdentifiedObject io in amisC2)
                    {
                        this.AmiTableItems.Add(new TableItem(io));
                        this.positionsAmi.Add(io.GlobalId, this.AmiTableItems.Count - 1);
                    }
                    break;
                case DMSType.SUBSTATION:
                    List<IdentifiedObject> substationsC3 = new List<IdentifiedObject>();

                    this.AmiTableItems.Clear();
                    List<IdentifiedObject> amisC3 = new List<IdentifiedObject>();

                    amisC3.AddRange(this.Model.GetSomeAmis(ParentGid));

                    foreach (IdentifiedObject io in amisC3)
                    {
                        this.AmiTableItems.Add(new TableItem(io));
                        this.positionsAmi.Add(io.GlobalId, this.AmiTableItems.Count - 1);
                    }
                    break;
            }

            Dictionary<long, DynamicMeasurement> changes = this.Model.GetChanges(this.positionsAmi.Keys.ToList());

            foreach (KeyValuePair<long, DynamicMeasurement> kvp in changes)
            {
                if (positionsAmi.ContainsKey(kvp.Key))
                {
                    AmiTableItems[positionsAmi[kvp.Key]].CurrentP = kvp.Value.CurrentP != -1 ? kvp.Value.CurrentP : AmiTableItems[positionsAmi[kvp.Key]].CurrentP;
                    AmiTableItems[positionsAmi[kvp.Key]].CurrentQ = kvp.Value.CurrentQ != -1 ? kvp.Value.CurrentQ : AmiTableItems[positionsAmi[kvp.Key]].CurrentQ;
                    AmiTableItems[positionsAmi[kvp.Key]].CurrentV = kvp.Value.CurrentV != -1 ? kvp.Value.CurrentV : AmiTableItems[positionsAmi[kvp.Key]].CurrentV;
                }
            }
        }

        public void SetModel(Model model)
        {
            this.Model = model;
            this.propertyCache = new Dictionary<string, PropertyInfo>();
            columnFilters = new Dictionary<string, string>();
            columnFilters[DataGridHeader.Name.ToString()] = string.Empty;
            columnFilters[DataGridHeader.Type.ToString()] = string.Empty;
            this.ViewAmiTableItems = new CollectionViewSource { Source = this.AmiTableItems }.View;
            this.ViewAmiTableItems = CollectionViewSource.GetDefaultView(this.AmiTableItems);
        }

        #region filter

        public void OnFilterApply()
        {
            this.ViewAmiTableItems = CollectionViewSource.GetDefaultView(this.Model.AmiTableItems);

            if (this.ViewAmiTableItems != null)
            {
                this.ViewAmiTableItems.Filter = delegate (object item)
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

        public ObservableCollection<TableItem> AmiTableItems
        {
            get
            {
                return amiTableItems;
            }

            set
            {
                amiTableItems = value;
                RaisePropertyChanged("AmiTableItems");
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

        private void CheckForUpdates()
        {
            while(true)
            {
                if (this.Model.NewChangesAvailable(this.timeOfLastUpdate))
                {
                    this.timeOfLastUpdate = this.Model.GetTimeOfTheLastUpdate();
                    Dictionary<long, DynamicMeasurement> changes = this.Model.GetChanges(this.positionsAmi.Keys.ToList());

                    foreach (KeyValuePair<long, DynamicMeasurement> kvp in changes)
                    {
                        if (positionsAmi.ContainsKey(kvp.Key))
                        {
                            AmiTableItems[positionsAmi[kvp.Key]].CurrentP = kvp.Value.CurrentP != -1 ? kvp.Value.CurrentP : AmiTableItems[positionsAmi[kvp.Key]].CurrentP;
                            AmiTableItems[positionsAmi[kvp.Key]].CurrentQ = kvp.Value.CurrentQ != -1 ? kvp.Value.CurrentQ : AmiTableItems[positionsAmi[kvp.Key]].CurrentQ;
                            AmiTableItems[positionsAmi[kvp.Key]].CurrentV = kvp.Value.CurrentV != -1 ? kvp.Value.CurrentV : AmiTableItems[positionsAmi[kvp.Key]].CurrentV;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public void Dispose()
        {
            this.checkIfThereAreNewUpdates.Abort();
        }
    }
}

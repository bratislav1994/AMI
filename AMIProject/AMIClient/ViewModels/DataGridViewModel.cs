using AMIClient.HelperClasses;
using AvalonDockMVVM.ViewModel;
using FTN.Common;
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
using System.Windows.Media;
using TC57CIM.IEC61970.Core;

namespace AMIClient.ViewModels
{
    public class DataGridViewModel : AvalonDockMVVM.ViewModel.DockWindowViewModel, INotifyPropertyChanged
    {
        private static DataGridViewModel instance;
        private Model model;
        private Dictionary<string, string> columnFilters;
        private Dictionary<string, PropertyInfo> propertyCache;
        private string nameFilter = string.Empty;
        private string typeFilter = string.Empty;
        private ObservableCollection<TableItem> tableItems;
        private Thread checkIfThereAreNewUpdates;
        private DateTime timeOfLastUpdateMeas = DateTime.Now;

        public DataGridViewModel()
        {
            this.tableItems = new ObservableCollection<TableItem>();
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

        public ObservableCollection<TableItem> TableItems
        {
            get
            {
                return tableItems;
            }

            set
            {
                tableItems = value;
                RaisePropertyChanged("TableItems");
            }
        }

        public static DataGridViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataGridViewModel();
                }

                return instance;
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

        public void SetModel(Model model)
        {
            this.Model = model;
            this.propertyCache = new Dictionary<string, PropertyInfo>();
            columnFilters = new Dictionary<string, string>();
            columnFilters[DataGridHeader.Name.ToString()] = string.Empty;
            columnFilters[DataGridHeader.Type.ToString()] = string.Empty;
            this.Model.ViewTableItems = new CollectionViewSource { Source = this.TableItems }.View;
            this.Model.ViewTableItems = CollectionViewSource.GetDefaultView(this.TableItems);
        }

        #region filter

        public void OnFilterApply()
        {
            this.Model.ViewTableItems = CollectionViewSource.GetDefaultView(this.TableItems);

            if (this.Model.ViewTableItems != null)
            {
                this.Model.ViewTableItems.Filter = delegate (object item)
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

        private void CheckForUpdates()
        {
            while (true)
            {
                if (this.Model != null)
                {
                    if (this.Model.NewChangesAvailableMeas(this.timeOfLastUpdateMeas))
                    {
                        this.timeOfLastUpdateMeas = this.Model.GetTimeOfTheLastUpdateMeas();
                        List<TableItem> changes = this.Model.GetChangesMeas();

                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            this.TableItems.Clear();

                            foreach (TableItem ti in changes)
                            {
                                ti.Status = ti.IsAlarm ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                                this.TableItems.Add(ti);
                            }
                        });
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private ICommand showAmisCommand;

        public ICommand ShowAmisCommand
        {
            get
            {
                return this.showAmisCommand ?? (this.showAmisCommand = new DelegateCommand<object>(this.ShowAmisAction, param => true));
            }
        }

        private void ShowAmisAction(object io)
        {
            if (io != null)
            {
                IdentifiedObject idObj = (IdentifiedObject)io;
                DMSType type = this.GetDmsTypeFromGid(idObj.GlobalId);
                AmiDataGridViewModel amiDG = new AmiDataGridViewModel() { Model = this.Model, Title = idObj.Name, ParentType = type, ParentGid = idObj.GlobalId };
                amiDG.SetModel(Model);
                var doc = new List<DockWindowViewModel>();
                doc.Add(amiDG);
                NetworkPreviewViewModel.Instance.DockManagerViewModel.Adding(doc);
            }
        }

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

            if (io != null)
            {
                switch (GetDmsTypeFromGid(io.GlobalId))
                {
                    case DMSType.ENERGYCONS:
                        NetworkPreviewViewModel.Instance.SelectedAMIAction(io, resolution);
                        break;
                    case DMSType.GEOREGION:
                        NetworkPreviewViewModel.Instance.ChartViewForGeoRegion(resolution, io.GlobalId, io.Name);
                        break;
                    case DMSType.SUBGEOREGION:
                        NetworkPreviewViewModel.Instance.ChartViewForSubGeoRegion(resolution, io.GlobalId, io.Name);
                        break;
                    case DMSType.SUBSTATION:
                        NetworkPreviewViewModel.Instance.ChartViewForSubstation(resolution, io.GlobalId, io.Name);
                        break;
                }
            }
        }

        private DMSType GetDmsTypeFromGid(long gid)
        {
            return (DMSType)(gid >> 32);
        }

        private ICommand consumptionCommand;

        public ICommand ConsumptionCommand
        {
            get
            {
                return this.consumptionCommand ?? (this.consumptionCommand = new DelegateCommand<object>(this.SelectedAMIConsumptionStatistic, param => true));
            }
        }

        private void SelectedAMIConsumptionStatistic(object selected)
        {
            IdentifiedObject io = ((IdentifiedObject)selected);

            switch (GetDmsTypeFromGid(io.GlobalId))
            {
                case DMSType.GEOREGION:
                    NetworkPreviewViewModel.Instance.ConsumptionStatisticForGeoRegion(io.GlobalId, io.Name);
                    break;
                case DMSType.SUBGEOREGION:
                    NetworkPreviewViewModel.Instance.ConsumptionStatisticForSubGeoRegion(io.GlobalId, io.Name);
                    break;
                case DMSType.SUBSTATION:
                    NetworkPreviewViewModel.Instance.ConsumptionStatisticForSubstation(io.GlobalId, io.Name);
                    break;
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
    }
}

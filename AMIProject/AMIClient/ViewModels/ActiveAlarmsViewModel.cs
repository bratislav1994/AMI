using AMIClient.Classes;
using AMIClient.HelperClasses;
using FTN.Common;
using FTN.Common.ClassesForAlarmDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMIClient.ViewModels
{
    public class ActiveAlarmsViewModel : AvalonDockMVVM.ViewModel.DockWindowViewModel, INotifyPropertyChanged
    {
        private static ActiveAlarmsViewModel instance;
        private Model model;
        private Dictionary<string, string> columnFilters;
        private string consumerFilter = string.Empty;
        private string typeVoltageFilter = string.Empty;
        private string georegionFilter = string.Empty;
        private ObservableCollection<ActiveAlarm> tableItemsForActiveAlarm = new ObservableCollection<ActiveAlarm>();
        private DateTime timeOfLastUpdateAlarm = DateTime.Now;
        private Thread checkIfThereAreNewUpdates;

        public ActiveAlarmsViewModel()
        {
            this.checkIfThereAreNewUpdates = new Thread(() => CheckForUpdates());
            this.checkIfThereAreNewUpdates.Start();
        }

        public void SetModel(Model model)
        {
            this.Model = model;
            columnFilters = new Dictionary<string, string>();
            columnFilters[DataGridAlarmHeader.Consumer.ToString()] = string.Empty;
            columnFilters[DataGridAlarmHeader.TypeVoltage.ToString()] = string.Empty;
            columnFilters[DataGridAlarmHeader.Georegion.ToString()] = string.Empty;
            this.Model.ViewTableItemsForActiveAlarm = new CollectionViewSource { Source = this.TableItemsForActiveAlarm }.View;
            this.Model.ViewTableItemsForActiveAlarm = CollectionViewSource.GetDefaultView(this.TableItemsForActiveAlarm);
            List<ActiveAlarm> changes = this.Model.GetChangesAlarm();

            foreach (ActiveAlarm alarm in changes)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.TableItemsForActiveAlarm.Add(alarm);
                });
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

        public string ConsumerFilter
        {
            get
            {
                return consumerFilter;
            }

            set
            {
                consumerFilter = value;
                this.RaisePropertyChanged("ConsumerFilter");
                columnFilters[DataGridAlarmHeader.Consumer.ToString()] = this.consumerFilter;
                this.OnFilterApply();
            }
        }

        public string TypeVoltageFilter
        {
            get
            {
                return typeVoltageFilter;
            }

            set
            {
                typeVoltageFilter = value;
                this.RaisePropertyChanged("TypeVoltageFilter");
                columnFilters[DataGridAlarmHeader.TypeVoltage.ToString()] = this.typeVoltageFilter;
                this.OnFilterApply();
            }
        }

        public string GeoregionFilter
        {
            get
            {
                return georegionFilter;
            }

            set
            {
                georegionFilter = value;
                this.RaisePropertyChanged("GeoregionFilter");
                columnFilters[DataGridAlarmHeader.Georegion.ToString()] = this.georegionFilter;
                this.OnFilterApply();
            }
        }

        public ObservableCollection<ActiveAlarm> TableItemsForActiveAlarm
        {
            get
            {
                return tableItemsForActiveAlarm;
            }

            set
            {
                tableItemsForActiveAlarm = value;
                RaisePropertyChanged("TableItemsForActiveAlarm");
            }
        }

        #region filter

        public void OnFilterApply()
        {
            this.Model.ViewTableItemsForActiveAlarm = CollectionViewSource.GetDefaultView(this.Model.TableItemsForActiveAlarm);

            if (this.Model.ViewTableItemsForActiveAlarm != null)
            {
                this.Model.ViewTableItemsForActiveAlarm.Filter = delegate (object item)
                {
                    bool show = true;

                    foreach (KeyValuePair<string, string> filter in columnFilters)
                    {
                        bool containsFilter = false;

                        if (filter.Key.Equals(DataGridAlarmHeader.Consumer.ToString()))
                        {
                            containsFilter = ((ActiveAlarm)item).Consumer.IndexOf(ConsumerFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }
                        else if (filter.Key.Equals(DataGridAlarmHeader.TypeVoltage.ToString()))
                        {
                            FTN.Common.TypeVoltage type = ((ActiveAlarm)item).TypeVoltage;
                            containsFilter = EnumDescription.GetEnumDescription(type).IndexOf(TypeVoltageFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }
                        else if (filter.Key.Equals(DataGridAlarmHeader.Georegion.ToString()))
                        {
                            containsFilter = ((ActiveAlarm)item).Georegion.IndexOf(GeoregionFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
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
                    if (this.Model.NewChangesAvailableAlarm(this.timeOfLastUpdateAlarm))
                    {
                        this.timeOfLastUpdateAlarm = this.Model.GetTimeOfTheLastUpdateAlarm();
                        List<ActiveAlarm> changes = this.Model.GetChangesAlarm();

                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            this.TableItemsForActiveAlarm.Clear();

                            foreach (ActiveAlarm alarm in changes)
                            {
                                this.TableItemsForActiveAlarm.Add(alarm);
                            }
                        });
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
    }
}

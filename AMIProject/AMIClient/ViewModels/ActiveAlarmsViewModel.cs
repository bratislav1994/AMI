using AMIClient.Classes;
using AMIClient.HelperClasses;
using FTN.Common.ClassesForAlarmDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMIClient.ViewModels
{
    public class ActiveAlarmsViewModel : AlarmViewModel, INotifyPropertyChanged
    {
        private static AlarmSummariesViewModel instance;
        private Model model;
        private Dictionary<string, string> columnFilters;
        private string consumerFilter = string.Empty;
        private string statusFilter = string.Empty;
        private string typeVoltageFilter = string.Empty;

        public ActiveAlarmsViewModel(Model model)
        {
            this.model = model;
        }

        public void SetModel(Model model)
        {
            this.Model = model;
            columnFilters = new Dictionary<string, string>();
            columnFilters[DataGridAlarmHeader.Consumer.ToString()] = string.Empty;
            columnFilters[DataGridAlarmHeader.Status.ToString()] = string.Empty;
            columnFilters[DataGridAlarmHeader.TypeVoltage.ToString()] = string.Empty;
            this.Model.ViewTableItemsForActiveAlarm = new CollectionViewSource { Source = this.Model.TableItemsForActiveAlarm }.View;
            this.Model.ViewTableItemsForActiveAlarm = CollectionViewSource.GetDefaultView(this.Model.TableItemsForActiveAlarm);
        }

        public static AlarmSummariesViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AlarmSummariesViewModel();
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

        public string StatusFilter
        {
            get
            {
                return statusFilter;
            }

            set
            {
                statusFilter = value;
                this.RaisePropertyChanged("StatusFilter");
                columnFilters[DataGridAlarmHeader.Status.ToString()] = this.statusFilter;
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
                            containsFilter = ((ActiveAlarm)item).Id.ToString().IndexOf(ConsumerFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }
                        else if (filter.Key.Equals(DataGridAlarmHeader.Status.ToString()))
                        {
                            FTN.Common.Status status = ((ActiveAlarm)item).Status;
                            containsFilter = EnumDescription.GetEnumDescription(status).IndexOf(StatusFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }
                        else if (filter.Key.Equals(DataGridAlarmHeader.TypeVoltage.ToString()))
                        {
                            FTN.Common.TypeVoltage type = ((ActiveAlarm)item).TypeVoltage;
                            containsFilter = EnumDescription.GetEnumDescription(type).IndexOf(TypeVoltageFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
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

using AMIClient.Classes;
using AMIClient.HelperClasses;
using AMIClient.PagginationCommands;
using FTN.Common.ClassesForAlarmDB;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using AMIClient.PagginationCommands.ResolvedAlarms;
using System.Collections.ObjectModel;
using FTN.Common;

namespace AMIClient.ViewModels
{
    public class ResolvedAlarmsViewModel : AlarmViewModel, INotifyPropertyChanged
    {
        private static ResolvedAlarmsViewModel instance;
        private Model model;
        private Dictionary<string, string> columnFilters;
        private string consumerFilter = string.Empty;
        private string statusFilter = string.Empty;
        private string typeVoltageFilter = string.Empty;
        private int itemPerPage = 10;
        private int enteredPage = 0;
        public ICommand PreviousCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand FirstCommand { get; private set; }
        public ICommand LastCommand { get; private set; }

        public ResolvedAlarmsViewModel()
        {
            NextCommand = new NextPageCommand(this);
            PreviousCommand = new PreviousPageCommand(this);
            FirstCommand = new FirstPageCommand(this);
            LastCommand = new LastPageCommand(this);
        }

        public void SetModel(Model model)
        {
            this.Model = model;
            columnFilters = new Dictionary<string, string>();
            columnFilters[DataGridAlarmHeader.Consumer.ToString()] = string.Empty;
            columnFilters[DataGridAlarmHeader.Status.ToString()] = string.Empty;
            columnFilters[DataGridAlarmHeader.TypeVoltage.ToString()] = string.Empty;
            this.Model.ViewTableItemsForResolvedAlarm = new CollectionViewSource { Source = this.Model.TableItemsForResolvedAlarm }.View;
            this.Model.ViewTableItemsForResolvedAlarm = CollectionViewSource.GetDefaultView(this.Model.TableItemsForResolvedAlarm);
        }

        public static ResolvedAlarmsViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResolvedAlarmsViewModel();
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

        public int EnteredPage
        {
            get
            {
                return this.enteredPage;
            }

            set
            {
                this.enteredPage = value;
                this.RaisePropertyChanged("EnteredPage");
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get { return _totalPages; }
            private set
            {
                _totalPages = value;
                this.RaisePropertyChanged("TotalPages");
            }
        }

        #region Pagination Methods
        public void ShowNextPage()
        {
            this.Model.TableItemsForResolvedAlarm.Clear();
            this.EnteredPage++;
            List<ResolvedAlarm> ret = this.Model.CEQueryProxy.GetResolvedAlarms((this.EnteredPage - 1) * this.itemPerPage, this.itemPerPage);
            ret.ForEach(x => this.Model.TableItemsForResolvedAlarm.Add(x));
        }

        public void ShowPreviousPage()
        {
            this.Model.TableItemsForResolvedAlarm.Clear();
            this.EnteredPage--;
            List<ResolvedAlarm> ret = this.Model.CEQueryProxy.GetResolvedAlarms((this.EnteredPage - 1) * this.itemPerPage, this.itemPerPage);
            ret.ForEach(x => this.Model.TableItemsForResolvedAlarm.Add(x));
        }

        public void ShowFirstPage()
        {
            this.Model.TableItemsForResolvedAlarm.Clear();
            this.EnteredPage = 1;
            List<ResolvedAlarm> ret = this.Model.CEQueryProxy.GetResolvedAlarms((this.EnteredPage - 1) * this.itemPerPage, this.itemPerPage);
            ret.ForEach(x => this.Model.TableItemsForResolvedAlarm.Add(x));
        }

        public void ShowLastPage()
        {
            this.Model.TableItemsForResolvedAlarm.Clear();
            this.EnteredPage = TotalPages;
            List<ResolvedAlarm> ret = this.Model.CEQueryProxy.GetResolvedAlarms((this.EnteredPage - 1) * this.itemPerPage, this.itemPerPage);
            ret.ForEach(x => this.Model.TableItemsForResolvedAlarm.Add(x));
        }

        #endregion

        private ICommand enterCommand;

        public ICommand EnterCommand
        {
            get
            {
                return this.enterCommand ?? (this.enterCommand = new DelegateCommand(this.EnterAction));
            }
        }

        private void EnterAction()
        {
            if (this.TotalPages >= this.EnteredPage)
            {
                List<ResolvedAlarm> ret = this.Model.CEQueryProxy.GetResolvedAlarms((this.EnteredPage - 1) * this.itemPerPage, this.itemPerPage);
                ret.ForEach(x => this.Model.TableItemsForResolvedAlarm.Add(x));
            }
        }

        private ICommand refreshCommand;

        public ICommand RefreshCommand
        {
            get
            {
                return this.refreshCommand ?? (this.refreshCommand = new DelegateCommand(this.RefreshHistory));
            }
        }

        private void RefreshHistory()
        {
            this.Model.TableItemsForResolvedAlarm.Clear();
            int totalNumberOfAlarms = this.Model.CEQueryProxy.GetTotalPageCount();

            if (totalNumberOfAlarms % this.itemPerPage == 0)
            {
                this.TotalPages = totalNumberOfAlarms / this.itemPerPage;
            }
            else
            {
                this.TotalPages = totalNumberOfAlarms / this.itemPerPage + 1;
            }
           
            if (this.TotalPages != 0)
            {
                if (this.EnteredPage == 0 || this.EnteredPage > this.TotalPages || this.EnteredPage < 0)
                {
                    this.EnteredPage = 1;
                }

                List<ResolvedAlarm> ret = this.Model.CEQueryProxy.GetResolvedAlarms((this.EnteredPage - 1) * this.itemPerPage, this.itemPerPage);
                ret.ForEach(x => this.Model.TableItemsForResolvedAlarm.Add(x));
            }
        }

        #region filter

        public void OnFilterApply()
        {
            this.Model.ViewTableItemsForResolvedAlarm = CollectionViewSource.GetDefaultView(this.Model.TableItemsForResolvedAlarm);

            if (this.Model.ViewTableItemsForResolvedAlarm != null)
            {
                this.Model.ViewTableItemsForResolvedAlarm.Filter = delegate (object item)
                {
                    bool show = true;

                    foreach (KeyValuePair<string, string> filter in columnFilters)
                    {
                        bool containsFilter = false;

                        if (filter.Key.Equals(DataGridAlarmHeader.Consumer.ToString()))
                        {
                            containsFilter = ((ResolvedAlarm)item).Id.ToString().IndexOf(ConsumerFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }
                        else if (filter.Key.Equals(DataGridAlarmHeader.Status.ToString()))
                        {
                            FTN.Common.Status status = ((ResolvedAlarm)item).Status;
                            containsFilter = EnumDescription.GetEnumDescription(status).IndexOf(StatusFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                        }
                        else if (filter.Key.Equals(DataGridAlarmHeader.TypeVoltage.ToString()))
                        {
                            FTN.Common.TypeVoltage type = ((ResolvedAlarm)item).TypeVoltage;
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
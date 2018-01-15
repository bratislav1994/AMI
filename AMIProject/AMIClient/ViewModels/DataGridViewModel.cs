using AMIClient.HelperClasses;
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

namespace AMIClient.ViewModels
{
    public class DataGridViewModel : INotifyPropertyChanged
    {
        private static DataGridViewModel instance;
        private Model model;
        private object tableItem;
        private Dictionary<string, string> columnFilters;
        private Dictionary<string, PropertyInfo> propertyCache;
        private string nameFilter = string.Empty;

        public DataGridViewModel()
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
            this.Model.ViewTableItems = new CollectionViewSource { Source = this.Model.TableItems }.View;
            this.Model.ViewTableItems = CollectionViewSource.GetDefaultView(this.Model.TableItems);
        }

        #region filter

        public void OnFilterApply()
        {
            this.Model.ViewTableItems = CollectionViewSource.GetDefaultView(this.Model.TableItems);

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

        public object TableItem
        {
            get
            {
                return tableItem;
            }

            set
            {
                if (value != null)
                {
                    if (((TableItem)value).Type == HelperClasses.DataGridType.ENERGY_CONSUMER)
                    {
                        tableItem = value;
                    }
                    else
                    {
                        tableItem = null;
                    }
                }
                else
                {
                    tableItem = value;
                }
                
                RaisePropertyChanged("TableItem");
            }
        }

        private void SelectedAMIAction(object ami)
        {
            NetworkPreviewViewModel.Instance.SelectedAMIAction(ami, 1);
        }

        private void SelectedAMIHourAction(object ami)
        {
            NetworkPreviewViewModel.Instance.SelectedAMIAction(ami, 2);
        }

        private void SelectedAMIDayAction(object ami)
        {
            NetworkPreviewViewModel.Instance.SelectedAMIAction(ami, 3);
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

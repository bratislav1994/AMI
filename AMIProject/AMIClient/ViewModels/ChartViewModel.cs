using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClient.ViewModels
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        private static ChartViewModel instance;
        private List<KeyValuePair<DateTime, double>> dataHistory;
        private string timePeriod;
        private ObservableCollection<string> timePeriods;
        private DelegateCommand showDataCommand;

        public ChartViewModel()
        {
            dataHistory = new List<KeyValuePair<DateTime, double>>();
            timePeriods = new ObservableCollection<string>() { "hour", "day", "week" };
        }

        public static ChartViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ChartViewModel();
                }

                return instance;
            }
        }

        public List<KeyValuePair<DateTime, double>> DataHistory
        {
            get
            {
                if (this.dataHistory == null)
                {
                    this.dataHistory = new List<KeyValuePair<DateTime, double>>();
                }

                return this.dataHistory;
            }

            set
            {
                this.dataHistory = value;
            }
        }

        public string TimePeriod
        {
            get
            {
                return timePeriod;
            }

            set
            {
                timePeriod = value;
                RaisePropertyChanged("TimePeriod");
            }
        }

        public ObservableCollection<string> TimePeriods
        {
            get
            {
                return timePeriods;
            }

            set
            {
                timePeriods = value;
                RaisePropertyChanged("TimePeriods");
            }
        }

        public DelegateCommand ShowDataCommand
        {
            get
            {
                if (this.showDataCommand == null)
                {
                    this.showDataCommand = new DelegateCommand(this.ShowCommandAction, this.CanShowDataExecute);
                }

                return this.showDataCommand;
            }
        }

        private bool CanShowDataExecute()
        {
            return !string.IsNullOrWhiteSpace(this.TimePeriod);
        }

        private void ShowCommandAction()
        {
            //try
            //{
            //    List<KeyValuePair<DateTime, double>> temp = this.Client.GetMeasurements(this.SelectedItem.MRID);
            //    StringBuilder allHist = new StringBuilder();

            //    if (temp.Count > 10)
            //    {
            //        List<KeyValuePair<DateTime, double>> lastFive = temp.GetRange(temp.Count - 10, 10);

            //        foreach (KeyValuePair<DateTime, double> kvp in lastFive)
            //        {
            //            this.DataHistory.Add(kvp);
            //        }
            //    }
            //    else
            //    {
            //        this.DataHistory = temp;
            //    }

            //    foreach (KeyValuePair<DateTime, double> kvp in temp)
            //    {
            //        allHist.AppendLine(kvp.Key + ", " + kvp.Value + "W");
            //    }

            //    this.AllHistory = allHist.ToString();
            //    this.showWin = new ShowDataWindow(this.Client.DataContext);
            //    this.showWin.ShowDialog();
            //}
            //catch
            //{
            //    if (!this.isTest)
            //    {
            //        MessageBox.Show("Error during getting measurement for selected generator.");
            //    }

            //    // this.SelectedItem = null;
            //    this.DataHistory = null;
            //    this.ShowDataCommand.RaiseCanExecuteChanged();
            //    this.ClickEditCommand.RaiseCanExecuteChanged();
            //    this.RemoveCommand.RaiseCanExecuteChanged();
            //}
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

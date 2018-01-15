using AMIClient.View;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMIClient.ViewModels
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        private static ChartViewModel instance;
        private List<KeyValuePair<DateTime, float>> dataHistoryP;
        private List<KeyValuePair<DateTime, float>> dataHistoryQ;
        private List<KeyValuePair<DateTime, float>> dataHistoryV;
        private string fromPeriod;
        private string toPeriod;
        private DelegateCommand showDataCommand;
        private Model model;
        private bool fromPeriodEntered = false;
        private bool toPeriodEntered = false;
        private List<long> amiGids;
        private Statistics statistics;
        private Visibility datePick = Visibility.Hidden;
        private Visibility dateTimePick = Visibility.Hidden;
        private int interval;

        public ChartViewModel()
        {
            dataHistoryP = new List<KeyValuePair<DateTime, float>>();
            dataHistoryQ = new List<KeyValuePair<DateTime, float>>();
            dataHistoryV = new List<KeyValuePair<DateTime, float>>();
            this.ShowDataCommand.RaiseCanExecuteChanged();
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

        public List<KeyValuePair<DateTime, float>> DataHistoryP
        {
            get
            {
                if (this.dataHistoryP == null)
                {
                    this.dataHistoryP = new List<KeyValuePair<DateTime, float>>();
                }

                return this.dataHistoryP;
            }

            set
            {
                this.dataHistoryP = value;
                RaisePropertyChanged("DataHistoryP");
            }
        }

        public List<KeyValuePair<DateTime, float>> DataHistoryQ
        {
            get
            {
                if (this.dataHistoryQ == null)
                {
                    this.dataHistoryQ = new List<KeyValuePair<DateTime, float>>();
                }

                return this.dataHistoryQ;
            }

            set
            {
                this.dataHistoryQ = value;
                RaisePropertyChanged("DataHistoryQ");
            }
        }

        public List<KeyValuePair<DateTime, float>> DataHistoryV
        {
            get
            {
                if (this.dataHistoryV == null)
                {
                    this.dataHistoryV = new List<KeyValuePair<DateTime, float>>();
                }

                return this.dataHistoryV;
            }

            set
            {
                this.dataHistoryV = value;
                RaisePropertyChanged("DataHistoryV");
            }
        }

        public string FromPeriod
        {
            get
            {
                return fromPeriod;
            }

            set
            {
                fromPeriod = value;
                this.fromPeriodEntered = !string.IsNullOrEmpty(this.fromPeriod) ? true : false;
                this.ShowDataCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("FromPeriod");
            }
        }

        public string ToPeriod
        {
            get
            {
                return toPeriod;
            }

            set
            {
                toPeriod = value;
                this.toPeriodEntered = !string.IsNullOrEmpty(this.toPeriod) ? true : false;
                this.ShowDataCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("ToPeriod");
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

        public Visibility DateTimePick
        {
            get
            {
                return dateTimePick;
            }
            set
            {
                dateTimePick = value;
            }
        }

        public Visibility DatePick
        {
            get
            {
                return datePick;
            }
            set
            {
                datePick = value;
            }
        }

        public int Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
            }
        }

        public List<long> AmiGids
        {
            get
            {
                return amiGids;
            }

            set
            {
                amiGids = value;
            }
        }

        public Statistics Statistics
        {
            get
            {
                return statistics;
            }

            set
            {
                statistics = value;
                RaisePropertyChanged("Statistics");
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
            fromPeriodEntered = DateTimeValidation(FromPeriod) ? true : false;
            toPeriodEntered = DateTimeValidation(ToPeriod) ? true : false;

            if (!fromPeriodEntered && !toPeriodEntered)
            {
                return false;
            }

            if (fromPeriodEntered && toPeriodEntered)
            {
                return DateTime.Compare(DateTime.Parse(FromPeriod), DateTime.Parse(ToPeriod)) <= 0;
            }
            else if (fromPeriodEntered)
            {
                if (!string.IsNullOrEmpty(ToPeriod))
                {
                    return false;
                }

                return DateTime.Compare(DateTime.Parse(FromPeriod), DateTime.Now) <= 0;
            }
            else
            {
                if (!string.IsNullOrEmpty(FromPeriod))
                {
                    return false;
                }

                return DateTime.Compare(DateTime.MinValue, DateTime.Parse(ToPeriod)) <= 0;
            }
        }

        private bool DateTimeValidation(string dt)
        {
            if (string.IsNullOrEmpty(dt))
            {
                return false;
            }

            try
            {
                DateTime.Parse(dt);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void ShowCommandAction()
        {
            DateTime from = new DateTime(), to = new DateTime();
            from = fromPeriodEntered ? DateTime.Parse(FromPeriod) : DateTime.MinValue;
            to = toPeriodEntered ? DateTime.Parse(ToPeriod) : DateTime.Now;

            Tuple<List<DynamicMeasurement>, Statistics> measForChart = this.Model.GetMeasForChart(AmiGids, from, to);
            if(measForChart == null)
            {
                return;
            }
            List<KeyValuePair<DateTime, float>> tempP = new List<KeyValuePair<DateTime, float>>();
            List<KeyValuePair<DateTime, float>> tempQ = new List<KeyValuePair<DateTime, float>>();
            List<KeyValuePair<DateTime, float>> tempV = new List<KeyValuePair<DateTime, float>>();

            foreach (DynamicMeasurement dm in measForChart.Item1)
            {
                tempP.Add(new KeyValuePair<DateTime, float>(dm.TimeStamp, dm.CurrentP));
                tempQ.Add(new KeyValuePair<DateTime, float>(dm.TimeStamp, dm.CurrentQ));
                tempV.Add(new KeyValuePair<DateTime, float>(dm.TimeStamp, dm.CurrentV));
            }

            this.DataHistoryP = tempP;
            this.DataHistoryQ = tempQ;
            this.DataHistoryV = tempV;
            this.Statistics = measForChart.Item2;
        }

        public void SetGids(List<long> amiGids)
        {
            this.AmiGids = amiGids;
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
            this.DataHistoryP.Clear();
            this.DataHistoryQ.Clear();
            this.DataHistoryV.Clear();
            this.FromPeriod = string.Empty;
            this.ToPeriod = string.Empty;
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

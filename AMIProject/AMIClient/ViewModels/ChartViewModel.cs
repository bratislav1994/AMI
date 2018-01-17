using AMIClient.View;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
        private DelegateCommand showDataCommand;
        private Model model;
        private List<long> amiGids;
        private Statistics statistics;
        private Visibility datePick = Visibility.Hidden;
        private Visibility dateTimePick = Visibility.Hidden;
        private ResolutionType resolution;

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
                this.ShowDataCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("FromPeriod");
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

        public ResolutionType Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                resolution = value;

                if (resolution == ResolutionType.MINUTE)
                {
                    DateTimePick = Visibility.Visible;
                }
                else
                {
                    DatePick = Visibility.Visible;
                }
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
            return DateTimeValidation(FromPeriod);
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

        private DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks / d.Ticks) * d.Ticks);
        }

        private void ShowCommandAction()
        {
            DateTime from = DateTime.Parse(FromPeriod);

            switch (this.Resolution)
            {
                case ResolutionType.MINUTE:
                    from = RoundDown(DateTime.Parse(FromPeriod), TimeSpan.FromHours(1));
                    break;
                case ResolutionType.HOUR:
                    from = RoundDown(DateTime.Parse(FromPeriod), TimeSpan.FromDays(1));
                    break;
                case ResolutionType.DAY:
                    DateTime dt = new DateTime(from.Year, 1, 1);
                    from = dt;
                    break;
            }

            Tuple<List<Statistics>, Statistics> measForChart = this.Model.GetMeasForChart(AmiGids, from, this.Resolution);

            if (measForChart == null)
            {
                return;
            }

            List<KeyValuePair<DateTime, float>> tempP = new List<KeyValuePair<DateTime, float>>();
            List<KeyValuePair<DateTime, float>> tempQ = new List<KeyValuePair<DateTime, float>>();
            List<KeyValuePair<DateTime, float>> tempV = new List<KeyValuePair<DateTime, float>>();

            foreach (Statistics dm in measForChart.Item1)
            {
                tempP.Add(new KeyValuePair<DateTime, float>(dm.TimeStamp, dm.AvgP));
                tempQ.Add(new KeyValuePair<DateTime, float>(dm.TimeStamp, dm.AvgQ));
                tempV.Add(new KeyValuePair<DateTime, float>(dm.TimeStamp, dm.AvgV));
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

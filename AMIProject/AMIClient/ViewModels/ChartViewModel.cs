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
using LiveCharts;
using LiveCharts.Wpf;

namespace AMIClient.ViewModels
{
    public class ChartViewModel : AvalonDockMVVM.ViewModel.DockWindowViewModel, INotifyPropertyChanged
    {
        private static ChartViewModel instance;
        private string fromPeriod;
        private DelegateCommand showDataCommand;
        private Model model;
        private List<long> amiGids;
        private Statistics statistics;
        private Visibility datePick = Visibility.Hidden;
        private Visibility dateTimePick = Visibility.Hidden;
        private ResolutionType resolution;
        private SeriesCollection dataHistoryPX;
        private string[] dataHistoryPY;
        private SeriesCollection dataHistoryQX;
        private string[] dataHistoryQY;
        private SeriesCollection dataHistoryVX;
        private string[] dataHistoryVY;

        public ChartViewModel()
        {
            this.ShowDataCommand.RaiseCanExecuteChanged();
            DataHistoryPX = new SeriesCollection();
            DataHistoryQX = new SeriesCollection();
            DataHistoryVX = new SeriesCollection();

            DataHistoryPX.Add(new LineSeries
            {
                Values = new ChartValues<float>(),
                LineSmoothness = 1, //straight lines, 1 really smooth lines

            });
            DataHistoryQX.Add(new LineSeries
            {
                Values = new ChartValues<float>(),
                LineSmoothness = 1, //straight lines, 1 really smooth lines

            });
            DataHistoryVX.Add(new LineSeries
            {
                Values = new ChartValues<float>(),
                LineSmoothness = 1, //straight lines, 1 really smooth lines

            });
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

        public SeriesCollection DataHistoryPX
        {
            get
            {
                return dataHistoryPX;
            }

            set
            {
                dataHistoryPX = value;
                RaisePropertyChanged("DataHistoryPX");
            }
        }

        public string[] DataHistoryPY
        {
            get
            {
                return dataHistoryPY;
            }

            set
            {
                dataHistoryPY = value;
                RaisePropertyChanged("DataHistoryPY");
            }
        }

        public SeriesCollection DataHistoryQX
        {
            get
            {
                return dataHistoryQX;
            }

            set
            {
                dataHistoryQX = value;
                RaisePropertyChanged("DataHistoryQX");
            }
        }

        public string[] DataHistoryQY
        {
            get
            {
                return dataHistoryQY;
            }

            set
            {
                dataHistoryQY = value;
                RaisePropertyChanged("DataHistoryQY");
            }
        }

        public SeriesCollection DataHistoryVX
        {
            get
            {
                return dataHistoryVX;
            }

            set
            {
                dataHistoryVX = value;
                RaisePropertyChanged("DataHistoryVX");
            }
        }

        public string[] DataHistoryVY
        {
            get
            {
                return dataHistoryVY;
            }

            set
            {
                dataHistoryVY = value;
                RaisePropertyChanged("DataHistoryVY");
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
                var inputCulture = CultureInfo.CreateSpecificCulture("us-en");
                DateTime.Parse(dt, inputCulture);
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
            var inputCulture = CultureInfo.CreateSpecificCulture("us-en");
            DateTime from = DateTime.Parse(FromPeriod, inputCulture);

            switch (this.Resolution)
            {
                case ResolutionType.MINUTE:
                    from = RoundDown(DateTime.Parse(FromPeriod, inputCulture), TimeSpan.FromHours(1));
                    break;
                case ResolutionType.HOUR:
                    from = RoundDown(DateTime.Parse(FromPeriod, inputCulture), TimeSpan.FromDays(1));
                    break;
                case ResolutionType.DAY:
                    from = DateTime.Parse(FromPeriod, inputCulture);
                    from = from.AddDays(-from.Day + 1);
                    break;
            }

            Tuple<List<Statistics>, Statistics> measForChart = this.Model.GetMeasForChart(AmiGids, from, this.Resolution);

            DataHistoryPX[0].Values.Clear();
            DataHistoryQX[0].Values.Clear();
            DataHistoryVX[0].Values.Clear();
            

            if (measForChart == null)
            {
                DataHistoryPY = new string[0];
                DataHistoryQY = new string[0];
                DataHistoryVY = new string[0];

                return;
            }

            DataHistoryPY = new string[measForChart.Item1.Count];
            DataHistoryQY = new string[measForChart.Item1.Count];
            DataHistoryVY = new string[measForChart.Item1.Count];

            int cnt = -1;

            foreach (Statistics dm in measForChart.Item1)
            {
                DataHistoryPX[0].Values.Add(dm.AvgP);
                DataHistoryQX[0].Values.Add(dm.AvgQ);
                DataHistoryVX[0].Values.Add(dm.AvgV);
                DataHistoryPY[++cnt] = dm.TimeStamp.ToString();
                DataHistoryQY[cnt] = dm.TimeStamp.ToString();
                DataHistoryVY[cnt] = dm.TimeStamp.ToString();
            }

            this.Statistics = measForChart.Item2;
        }

        public void SetGids(List<long> amiGids)
        {
            this.AmiGids = amiGids;
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

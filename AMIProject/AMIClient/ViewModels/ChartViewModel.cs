﻿using AMIClient.View;
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
        private SeriesCollection dataHistoryPX;
        private string[] dataHistoryPY = new string[3];

        public ChartViewModel()
        {
            dataHistoryP = new List<KeyValuePair<DateTime, float>>();
            dataHistoryQ = new List<KeyValuePair<DateTime, float>>();
            dataHistoryV = new List<KeyValuePair<DateTime, float>>();
            this.ShowDataCommand.RaiseCanExecuteChanged();
            DataHistoryPX = new SeriesCollection();

            DataHistoryPX.Add(new LineSeries
            {
                //Values = new ChartValues<double> { 67, 12, 56, 99, -10 },
                LineSmoothness = 0, //straight lines, 1 really smooth lines
                
            });

            List<KeyValuePair<DateTime, float>> tempP = new List<KeyValuePair<DateTime, float>>();
            Statistics s1 = new Statistics();
            Statistics s2 = new Statistics();
            Statistics s3 = new Statistics();
            s1.TimeStamp = new DateTime(2018, 2, 8);
            s2.TimeStamp = new DateTime(2018, 2, 8);
            s3.TimeStamp = new DateTime(2018, 2, 8);
            s1.TimeStamp = s1.TimeStamp.AddHours(14);
            s2.TimeStamp = s2.TimeStamp.AddHours(15);
            s3.TimeStamp = s3.TimeStamp.AddHours(16);
            s1.TimeStamp.AddMinutes(15);
            s2.TimeStamp.AddMinutes(30);
            s3.TimeStamp.AddMinutes(45);
            s1.AvgP = 90;
            s2.AvgP = 20;
            s3.AvgP = 80;

            DataHistoryPX[0].Values = new ChartValues<float>();
            DataHistoryPX[0].Values.Add(s1.AvgP);
            DataHistoryPX[0].Values.Add(s2.AvgP);
            DataHistoryPX[0].Values.Add(s3.AvgP);
            int cnt = -1;
            DataHistoryPY[0] = s1.TimeStamp.Hour.ToString();
            DataHistoryPY[1] = s2.TimeStamp.Hour.ToString();
            DataHistoryPY[2] = s3.TimeStamp.Hour.ToString();
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
                return this.dataHistoryP;
            }

            set
            {
                this.dataHistoryP = value;
                RaisePropertyChanged("DataHistoryP");
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

        public List<KeyValuePair<DateTime, float>> DataHistoryQ
        {
            get
            {
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
            var inputCulture = CultureInfo.CreateSpecificCulture("us-en");
            DateTime from = DateTime.Parse(FromPeriod, inputCulture);

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
            DataHistoryPX[0].Values.Clear();
            DataHistoryPY = new string[measForChart.Item1.Count];
            int cnt = -1;

            foreach (Statistics dm in measForChart.Item1)
            {
                DataHistoryPX[0].Values.Add(dm.AvgP);
                DataHistoryPY[++cnt] = dm.TimeStamp.ToString();
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

using FTN.Common;
using FTN.Common.Filter;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AMIClient.ViewModels
{
    public class ConsumptionStatisticViewModel : AvalonDockMVVM.ViewModel.DockWindowViewModel, INotifyPropertyChanged
    {
        private static ConsumptionStatisticViewModel instance;
        private string fromPeriod;
        private DelegateCommand showDataCommand;
        private Model model;
        private List<long> amiGids;
        private Statistics statistics;
        private Visibility datePick = Visibility.Visible;
        private Visibility dateTimePick = Visibility.Hidden;
        private ResolutionType resolution = ResolutionType.DAY;
        private bool seasonChecked = false;
        private List<Season> seasonCb;
        private Season seasonSelected;
        private bool isSeasonCheckBoxEnabled;
        private bool typeOfDayChecked = false;
        private List<Classes.DayOfWeek> typeOfDayCb;
        private TypeOfDay typeOfDaySelected;
        private bool isTypeOfDayCheckBoxEnabled;
        private bool consumerTypeChecked = false;
        private bool isSpecificCheckBoxEnabled;
        private bool specificDayChecked = false;
        private List<int> yearCb;
        private List<string> monthCb;
        private Dictionary<string, int> months;
        private List<int> dayCb;
        private int selectedFromYear = -1;
        private int selectedToYear = -1;
        private string selectedMonth = "";
        private int selectedDay = -1;
        private List<ConsumerType> consumerTypeCb;
        private ConsumerType consumerTypeSelected;
        private bool isConsumerTypeCheckBoxEnabled;
        private SeriesCollection dataHistoryPX;
        private string[] dataHistoryPY;
        private SeriesCollection dataHistoryQX;
        private string[] dataHistoryQY;
        private SeriesCollection dataHistoryVX;
        private string[] dataHistoryVY;
        private List<int> days;

        public ConsumptionStatisticViewModel()
        {
            SeasonCb = new List<Season>() { Season.SUMMER, Season.WINTER };
            TypeOfDayCb = new List<AMIClient.Classes.DayOfWeek>() { new Classes.DayOfWeek(DayOfWeek.Monday) , new Classes.DayOfWeek(DayOfWeek.Tuesday), new Classes.DayOfWeek(DayOfWeek.Wednesday),
                                                                    new Classes.DayOfWeek(DayOfWeek.Thursday), new Classes.DayOfWeek(DayOfWeek.Friday), new Classes.DayOfWeek(DayOfWeek.Saturday),
                                                                    new Classes.DayOfWeek(DayOfWeek.Sunday)};
            ConsumerTypeCb = new List<ConsumerType>() { ConsumerType.HOUSEHOLD, ConsumerType.FIRM, ConsumerType.SHOPPING_CENTER };
            YearCb= new List<int>();
            months = new Dictionary<string, int>();
            days = new List<int>();
            int year = DateTime.Now.Year;

            for (int i = 1; i <= 31; i++)
            {
                days.Add(i);
            }

            DayCb = days;

            for (int i = 0; i < 30; i++)
            {
                YearCb.Add(year - i);
            }

            MonthCb = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList().GetRange(0,12);

            int j = 0;
            foreach(string name in MonthCb)
            {
                months.Add(name, ++j);
            }

            IsSeasonCheckBoxEnabled = true;
            IsTypeOfDayCheckBoxEnabled = true;
            IsSpecificCheckBoxEnabled = true;
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

        public static ConsumptionStatisticViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConsumptionStatisticViewModel();
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

        public bool SeasonChecked
        {
            get
            {
                return seasonChecked;
            }

            set
            {
                this.seasonChecked = value;
                RaisePropertyChanged("SeasonChecked");
            }
        }
        
        public List<Season> SeasonCb
        {
            get
            {
                return seasonCb;
            }

            set
            {
                seasonCb = value;
                RaisePropertyChanged("SeasonCb");
            }
        }

        public Season SeasonSelected
        {
            get
            {
                return seasonSelected;
            }

            set
            {
                seasonSelected = value;
                RaisePropertyChanged("SeasonSelected");
            }
        }

        public bool IsSeasonCheckBoxEnabled
        {
            get
            {
                return isSeasonCheckBoxEnabled;
            }

            set
            {
                isSeasonCheckBoxEnabled = value;
                RaisePropertyChanged("IsSeasonCheckBoxEnabled");
            }
        }
        
        public bool TypeOfDayChecked
        {
            get
            {
                return typeOfDayChecked;
            }

            set
            {
                this.typeOfDayChecked = value;
                RaisePropertyChanged("TypeOfDayChecked");
            }
        }

        public List<Classes.DayOfWeek> TypeOfDayCb
        {
            get
            {
                return typeOfDayCb;
            }

            set
            {
                typeOfDayCb = value;
                RaisePropertyChanged("TypeOfDayCb");
            }
        }

        public TypeOfDay TypeOfDaySelected
        {
            get
            {
                return typeOfDaySelected;
            }

            set
            {
                typeOfDaySelected = value;
                RaisePropertyChanged("TypeOfDaySelected");
            }
        }

        public bool IsTypeOfDayCheckBoxEnabled
        {
            get
            {
                return isTypeOfDayCheckBoxEnabled;
            }

            set
            {
                isTypeOfDayCheckBoxEnabled = value;
                RaisePropertyChanged("IsTypeOfDayCheckBoxEnabled");
            }
        }

        public bool ConsumerTypeChecked
        {
            get
            {
                return consumerTypeChecked;
            }

            set
            {
                consumerTypeChecked = value;
                if (value)
                {
                    ConsumerTypeSelected = ConsumerTypeCb[0];
                }
                RaisePropertyChanged("ConsumerTypeChecked");
            }
        }

        public List<ConsumerType> ConsumerTypeCb
        {
            get
            {
                return consumerTypeCb;
            }

            set
            {
                consumerTypeCb = value;

                if (value.Count == 1)
                {
                    this.ConsumerTypeSelected = ConsumerTypeCb[0];
                    this.consumerTypeChecked = true;
                }

                RaisePropertyChanged("ConsumerTypeCb");
            }
        }

        public ConsumerType ConsumerTypeSelected
        {
            get
            {
                return consumerTypeSelected;
            }

            set
            {
                consumerTypeSelected = value;
                RaisePropertyChanged("ConsumerTypeSelected");
            }
        }

        public bool IsConsumerTypeCheckBoxEnabled
        {
            get
            {
                return isConsumerTypeCheckBoxEnabled;
            }

            set
            {
                isConsumerTypeCheckBoxEnabled = value;
                RaisePropertyChanged("ConsumerTypeCheckBoxEnabled");
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

        public bool SpecificDayChecked
        {
            get
            {
                return specificDayChecked;
            }

            set
            {
                specificDayChecked = value;
                RaisePropertyChanged("SpecificDayChecked");
            }
        }

        public List<int> YearCb
        {
            get
            {
                return yearCb;
            }

            set
            {
                yearCb = value;
            }
        }

        public List<string> MonthCb
        {
            get
            {
                return monthCb;
            }

            set
            {
                monthCb = value;
            }
        }

        public List<int> DayCb
        {
            get
            {
                return dayCb;
            }

            set
            {
                dayCb = value;
                RaisePropertyChanged("DayCb");
            }
        }

        public int SelectedFromYear
        {
            get
            {
                return selectedFromYear;
            }

            set
            {
                selectedFromYear = value;
                RaisePropertyChanged("SelectedFromYear");
                this.ShowDataCommand.RaiseCanExecuteChanged();
            }
        }

        public int SelectedToYear
        {
            get
            {
                return selectedToYear;
            }

            set
            {
                selectedToYear = value;
                RaisePropertyChanged("SelectedToYear");
                this.ShowDataCommand.RaiseCanExecuteChanged();
            }
        }

        public string SelectedMonth
        {
            get
            {
                return selectedMonth;
            }

            set
            {
                selectedMonth = value;
                int numberOfDays = DateTime.DaysInMonth(2016, months[value]);
                DayCb = days.GetRange(0, numberOfDays);
                RaisePropertyChanged("SelectedMonth");

            }
        }

        public int SelectedDay
        {
            get
            {
                return selectedDay;
            }

            set
            {
                selectedDay = value;
                RaisePropertyChanged("SelectedDay");
            }
        }

        public bool IsSpecificCheckBoxEnabled
        {
            get
            {
                return isSpecificCheckBoxEnabled;
            }

            set
            {
                isSpecificCheckBoxEnabled = value;
                RaisePropertyChanged("IsSpecificCheckBoxEnabled");
            }
        }

        private bool CanShowDataExecute()
        {
            if (SelectedFromYear != -1 && SelectedToYear != -1)
            {
                if (SelectedFromYear > SelectedToYear)
                {
                    return false;
                }
            }

            return true;
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
            Filter filter = new Filter()
            {
                ConsumerHasValue = this.ConsumerTypeChecked ? true : false,
                TypeOfDayHasValue = this.TypeOfDayChecked ? true : false,
                SeasonHasValue = this.SeasonChecked ? true : false,
                ConsumerType = this.ConsumerTypeSelected,
                Season = this.SeasonSelected,
                YearFrom = SelectedFromYear == -1 ? 1 : SelectedFromYear,
                YearTo = SelectedToYear == -1 ? DateTime.Now.Year : SelectedToYear,
                Month = SelectedMonth == "" ? -1 : months[SelectedMonth],
                Day = SelectedDay,
                SpecificDayHasValue = this.SpecificDayChecked ? true : false
            };

            foreach (Classes.DayOfWeek day in TypeOfDayCb)
            {
                if (day.IsChecked)
                {
                    filter.TypeOfDay.Add(day.Name);
                }
            }

            Tuple<List<HourAggregation>, Statistics> measForChart = this.Model.GetMeasurementsForChartViewByFilter(AmiGids, filter);

            if (measForChart == null)
            {
                return;
            }
            
            DataHistoryPX[0].Values.Clear();
            DataHistoryQX[0].Values.Clear();
            DataHistoryVX[0].Values.Clear();
            DataHistoryPY = new string[measForChart.Item1.Count];
            DataHistoryQY = new string[measForChart.Item1.Count];
            DataHistoryVY = new string[measForChart.Item1.Count];
            int cnt = -1;

            foreach (Statistics dm in measForChart.Item1)
            {
                DataHistoryPX[0].Values.Add(dm.AvgP);
                DataHistoryQX[0].Values.Add(dm.AvgQ);
                DataHistoryVX[0].Values.Add(dm.AvgV);
                DataHistoryPY[++cnt] = dm.TimeStamp.Hour.ToString();
                DataHistoryQY[cnt] = dm.TimeStamp.Hour.ToString();
                DataHistoryVY[cnt] = dm.TimeStamp.Hour.ToString();
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

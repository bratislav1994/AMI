using FTN.Common;
using FTN.Common.Filter;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
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
        private List<KeyValuePair<DateTime, float>> dataHistoryP;
        private List<KeyValuePair<DateTime, float>> dataHistoryQ;
        private List<KeyValuePair<DateTime, float>> dataHistoryV;
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
        private List<TypeOfDay> typeOfDayCb;
        private TypeOfDay typeOfDaySelected;
        private bool isTypeOfDayCheckBoxEnabled;
        private bool consumerTypeChecked = false;
        private List<ConsumerType> consumerTypeCb;
        private ConsumerType consumerTypeSelected;
        private bool isConsumerTypeCheckBoxEnabled;

        public ConsumptionStatisticViewModel()
        {
            SeasonCb = new List<Season>() { Season.SUMMER, Season.WINTER };
            TypeOfDayCb = new List<TypeOfDay>() { TypeOfDay.WORKDAY, TypeOfDay.WEEKEND };
            ConsumerTypeCb = new List<ConsumerType>() { ConsumerType.HOUSEHOLD, ConsumerType.FIRM, ConsumerType.SHOPPING_CENTER };
            IsSeasonCheckBoxEnabled = true;
            IsTypeOfDayCheckBoxEnabled = true;
            dataHistoryP = new List<KeyValuePair<DateTime, float>>();
            dataHistoryQ = new List<KeyValuePair<DateTime, float>>();
            dataHistoryV = new List<KeyValuePair<DateTime, float>>();
            this.ShowDataCommand.RaiseCanExecuteChanged();
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

        public List<TypeOfDay> TypeOfDayCb
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
        
        private bool CanShowDataExecute()
        {
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
                TypeOfDay = this.TypeOfDaySelected,
                Season = this.SeasonSelected
            };

            Tuple<List<Statistics>, Statistics> measForChart = this.Model.GetMeasurementsForChartViewByFilter(AmiGids, filter);

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

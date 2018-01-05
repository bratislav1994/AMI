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
        private ChartWindow chartWin;
        private Model model;
        private bool fromPeriodEntered = false;
        private bool toPeriodEntered = false;
        private long amiGid;

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

                if (!string.IsNullOrEmpty(this.fromPeriod))
                {
                    this.fromPeriodEntered = true;
                }
                else
                {
                    this.fromPeriodEntered = false;
                }

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

                if (!string.IsNullOrEmpty(this.toPeriod))
                {
                    this.toPeriodEntered = true;
                }
                else
                {
                    this.toPeriodEntered = false;
                }

                this.ShowDataCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("ToPeriod");
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

        public long AmiGid
        {
            get
            {
                return amiGid;
            }

            set
            {
                amiGid = value;
            }
        }

        private bool CanShowDataExecute()
        {
            //return !string.IsNullOrWhiteSpace(this.TimePeriod);
            return this.fromPeriodEntered || this.toPeriodEntered;
        }

        private void ShowCommandAction()
        {
            DateTime from = new DateTime(), to = new DateTime();

            if (!string.IsNullOrEmpty(FromPeriod))
            {
                try
                {
                    from = DateTime.Parse(FromPeriod);
                }
                catch
                {
                    MessageBox.Show("druze ne valja ti datum");
                }
            }
            else
            {
                from = DateTime.MinValue;
            }

            if (!string.IsNullOrEmpty(ToPeriod))
            {
                try
                {
                    to = DateTime.Parse(ToPeriod);
                }
                catch
                {
                    MessageBox.Show("druze ne valja ti datum");
                }
            }
            else
            {
                to = DateTime.Now;
            }

            Tuple<List<DynamicMeasurement>, Statistics> measForChart = this.Model.GetMeasForChart(AmiGid, from, to);
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
        }

        public void OpenWindow(long amiGid)
        {
            this.AmiGid = amiGid;
            this.chartWin = new ChartWindow(this);
            this.chartWin.ShowDialog();
        }

        public void SetModel(Model model)
        {
            this.Model = model;
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

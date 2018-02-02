using AMIClient.Classes;
using AMIClient.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AMIClient.ViewModels
{
    public class AlarmSummariesViewModel : INotifyPropertyChanged
    {
        private static AlarmSummariesViewModel instance;
        private Model model;
        private ActiveAlarmsViewModel activeAlarmsVM;
        private ResolvedAlarmsViewModel resolvedAlarmsVM;
        private BindingList<AlarmViewModel> alarmViewModels;
        private AlarmViewModel selectedTab;

        public AlarmSummariesViewModel()
        {

        }

        public void SetModel(Model model)
        {
            this.Model = model;
            this.ActiveAlarmsVM = new ActiveAlarmsViewModel(model) { Header = "Active alarms" };
            this.ResolvedAlarmsVM = new ResolvedAlarmsViewModel(model) { Header = "Resolved alarms" };
            this.AlarmViewModels = new BindingList<AlarmViewModel>();
            this.AlarmViewModels.Add(this.ActiveAlarmsVM);
            this.AlarmViewModels.Add(this.ResolvedAlarmsVM);
            this.SelectedTab = this.AlarmViewModels[0];
        }

        public static AlarmSummariesViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AlarmSummariesViewModel();
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

        public ActiveAlarmsViewModel ActiveAlarmsVM
        {
            get
            {
                return activeAlarmsVM;
            }

            set
            {
                activeAlarmsVM = value;
                this.RaisePropertyChanged("ActiveAlarmsVM");
            }
        }

        public ResolvedAlarmsViewModel ResolvedAlarmsVM
        {
            get
            {
                return resolvedAlarmsVM;
            }

            set
            {
                resolvedAlarmsVM = value;
                this.RaisePropertyChanged("ResolvedAlarmsVM");
            }
        }

        public BindingList<AlarmViewModel> AlarmViewModels
        {
            get
            {
                return alarmViewModels;
            }

            set
            {
                alarmViewModels = value;
                this.RaisePropertyChanged("AlarmViewModels");
            }
        }

        public AlarmViewModel SelectedTab
        {
            get
            {
                return selectedTab;
            }

            set
            {
                selectedTab = value;
                this.RaisePropertyChanged("SelectedTab");
            }
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

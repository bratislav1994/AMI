using AMIClient.View;
using AvalonDockMVVM.ViewModel;
using FTN.Common.Logger;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClient.ViewModels
{
    public class MasterViewModel : INotifyPropertyChanged
    {
        private NetworkPreviewViewModel tvm;
        private AddCimXmlViewModel xmlvm;
        private DataGridViewModel dgVM;
        private Model model;

        public MasterViewModel()
        {

        }

        public AddCimXmlViewModel Xmlvm
        {
            get
            {
                return xmlvm;
            }

            set
            {
                xmlvm = value;
            }
        }

        public NetworkPreviewViewModel Tvm
        {
            get
            {
                return tvm;
            }

            set
            {
                tvm = value;
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

        public void Init()
        {
            Logger.Path = "Client.txt";
            Model = new Model();
            Model.Start();
            tvm = NetworkPreviewViewModel.Instance;
            tvm.SetModel(Model);
            xmlvm = AddCimXmlViewModel.Instance;
            this.DgVM = DataGridViewModel.Instance;
            this.DgVM.SetModel(Model);
            this.CurrentViewModel = Tvm;
        }

        private object currentViewModel;

        public object CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                currentViewModel = value;
                this.RaisePropertyChanged("CurrentViewModel");
            }
        }

        private DelegateCommand networkPreviewCommand;
        public DelegateCommand NetworkPreviewCommand
        {
            get
            {
                if (networkPreviewCommand == null)
                {
                    networkPreviewCommand = new DelegateCommand(NetworkPreviewAction);
                }

                return networkPreviewCommand;
            }
        }

        private void NetworkPreviewAction()
        {
            this.CurrentViewModel = Tvm;
        }

        private DelegateCommand addCimXmlCommand;
        public DelegateCommand AddCimXmlCommand
        {
            get
            {
                if (addCimXmlCommand == null)
                {
                    addCimXmlCommand = new DelegateCommand(AddCimXmlAction);
                }

                return addCimXmlCommand;
            }
        }

        private void AddCimXmlAction()
        {
            this.CurrentViewModel = Xmlvm;
        }

        private DelegateCommand activeAlarmsCommand;
        public DelegateCommand ActiveAlarmsCommand
        {
            get
            {
                if (activeAlarmsCommand == null)
                {
                    activeAlarmsCommand = new DelegateCommand(ActiveAlarmsAction);
                }

                return activeAlarmsCommand;
            }
        }

        private void ActiveAlarmsAction()
        {
            var doc = new List<DockWindowViewModel>();
            var activeAlarms = new ActiveAlarmsViewModel() { Title = "Active alarms" };
            activeAlarms.SetModel(Model);
            doc.Add(activeAlarms);
            this.Tvm.DockManagerViewModel.Adding(doc);
        }

        private DelegateCommand resolvedAlarmsCommand;
        public DelegateCommand ResolvedAlarmsCommand
        {
            get
            {
                if (resolvedAlarmsCommand == null)
                {
                    resolvedAlarmsCommand = new DelegateCommand(ResolvedAlarmsAction);
                }

                return resolvedAlarmsCommand;
            }
        }

        private void ResolvedAlarmsAction()
        {
            var doc = new List<DockWindowViewModel>();
            var resolvedAlarms = new ResolvedAlarmsViewModel() { Title = "Resolved alarms" };
            resolvedAlarms.SetModel(Model);
            doc.Add(resolvedAlarms);
            this.Tvm.DockManagerViewModel.Adding(doc);
        }

        public DataGridViewModel DgVM
        {
            get
            {
                return dgVM;
            }

            set
            {
                dgVM = value;
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

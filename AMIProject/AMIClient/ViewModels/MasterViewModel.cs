using AMIClient.View;
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
        private ChartViewModel chartVM;
        private Model model;

        public MasterViewModel()
        {
            Logger.Path = "Client.txt";
            Model = new Model();
            Model.Start();
            tvm = new NetworkPreviewViewModel();
            tvm.SetModel(Model);
            xmlvm = new AddCimXmlViewModel();
            chartVM = new ChartViewModel();
            this.CurrentViewModel = Tvm;
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

        public ChartViewModel ChartVM
        {
            get
            {
                return chartVM;
            }

            set
            {
                chartVM = value;
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

        private DelegateCommand chartCommand;
        public DelegateCommand ChartCommand
        {
            get
            {
                if (chartCommand == null)
                {
                    chartCommand = new DelegateCommand(ChartAction);
                }

                return chartCommand;
            }
        }

        private void ChartAction()
        {
            this.CurrentViewModel = ChartVM;
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

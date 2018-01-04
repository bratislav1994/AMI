using AMIClient.View;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient.ViewModels
{
    public class NetworkPreviewViewModel : INotifyPropertyChanged
    {
        private Model model;
        private ObservableCollection<RootElement> rootElements;
        private static NetworkPreviewViewModel instance;
        private EnergyConsumerForTable selectedAMI;
        private ChartWindow chartWin;
        private ChartViewModel cvm;

        public NetworkPreviewViewModel()
        {
            cvm = new ChartViewModel();
            rootElements = new ObservableCollection<RootElement>();
        }

        public static NetworkPreviewViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NetworkPreviewViewModel();
                }

                return instance;
            }
        }

        public ObservableCollection<RootElement> RootElements
        {
            get
            {
                return rootElements;
            }

            set
            {
                rootElements = value;
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

        public void SetModel(Model model)
        {
            this.Model = model;
            RootElements.Add(new RootElement(this.model));
            this.model.SetRoot(rootElements[0]);
            this.cvm.SetModel(model);
        }

        //private ICommand selectedAMICommand;

        //public ICommand SelectedAMICommand
        //{
        //    get { return selectedAMICommand ?? (selectedAMICommand = new DelegateCommand<string>(SelectedAMIAction)); }
        //}

        public EnergyConsumerForTable SelectedAMI
        {
            get
            {
                return selectedAMI;
            }

            set
            {
                selectedAMI = value;

                if (value != null)
                {
                    this.SelectedAMIAction();
                }

                RaisePropertyChanged("SelectedAMI");
            }
        }

        private void SelectedAMIAction()
        {
            cvm.OpenWindow(this.SelectedAMI.Ami.GlobalId);
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

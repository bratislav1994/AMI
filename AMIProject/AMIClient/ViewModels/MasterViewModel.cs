using AMIClient.View;
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
        private IModel model;

        public MasterViewModel()
        {
            model = new Model();
            tvm = new NetworkPreviewViewModel(model);
            xmlvm = new AddCimXmlViewModel();
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

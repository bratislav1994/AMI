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
using System.Windows.Controls;
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

        private RelayCommand individualAmiChartCommand;

        public ICommand IndividualAmiChartCommand
        {
            get
            {
                return this.individualAmiChartCommand ?? (this.individualAmiChartCommand = new RelayCommand(this.SelectedAMIAction, param => true));
            }
        }

        private void SelectedAMIAction(object gid)
        {
            cvm.OpenWindow((long)gid);
        }

        private RelayCommand groupAmiChartCommand;

        public ICommand GroupAMIChartCommand
        {
            get
            {
                return this.groupAmiChartCommand ?? (this.groupAmiChartCommand = new RelayCommand(this.SelectedAMIsAction, param => true));
            }
        }

        private void SelectedAMIsAction(object selectedTreeView)
        {
            object o = selectedTreeView;
            //TreeView selected = (TreeView)selectedTreeView;
            //RootElement root = (RootElement)selected.Items.CurrentItem;
            
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

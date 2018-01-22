using AMIClient.View;
using AvalonDockMVVM.ViewModel;
using FTN.Common;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public DockManagerViewModel DockManagerViewModel { get; private set; }
        private List<DockWindowViewModel> documents =  new List<DockWindowViewModel>();

        public NetworkPreviewViewModel()
        {
            rootElements = new ObservableCollection<RootElement>();
            DataGridViewModel.Instance.Title = "Table";
            documents.Add(DataGridViewModel.Instance);
            this.DockManagerViewModel = new DockManagerViewModel(documents);

            //TabItems.Add(new TabItem()
            //{
            //    Header = "Table",
            //    CurrentTab = DataGridViewModel.Instance,
            //    Exit = Visibility.Hidden
            //});

            //SelectedTab = TabItems.First();
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
        }

        public void SelectedAMIAction(object ami, ResolutionType resolution)
        {
            IdentifiedObject ec = (IdentifiedObject)ami;
            ChartViewModel chartVM = null;
            
            chartVM = new ChartViewModel() { Model = this.Model, Resolution = resolution };
            chartVM.SetGids(new List<long>() { ec.GlobalId });
            var doc = new List<DockWindowViewModel>();
            doc.Add(new ChartViewModel() { Title = ec.Name });

            this.DockManagerViewModel.Adding(doc);

            //TabItems.Add(new TabItem()
            //{
            //    Header = ec.Name,
            //    CurrentTab = chartVM
            //});

            //SelectedTab = TabItems.Last();
        }

        private ICommand groupAmiChartMinCommand;

        public ICommand GroupAmiChartMinCommand
        {
            get
            {
                return this.groupAmiChartMinCommand ?? (this.groupAmiChartMinCommand = new DelegateCommand<object>(this.CommandActionForMin, param => true));
            }
        }

        private void CommandActionForMin(object selected)
        {
            this.SelectedAMIsAction(selected, ResolutionType.MINUTE);
        }

        private ICommand groupAmiChartHourCommand;

        public ICommand GroupAmiChartHourCommand
        {
            get
            {
                return this.groupAmiChartHourCommand ?? (this.groupAmiChartHourCommand = new DelegateCommand<object>(this.CommandActionForHour, param => true));
            }
        }

        private void CommandActionForHour(object selected)
        {
            this.SelectedAMIsAction(selected, ResolutionType.HOUR);
        }

        private ICommand groupAmiChartDayCommand;

        public ICommand GroupAmiChartDayCommand
        {
            get
            {
                return this.groupAmiChartDayCommand ?? (this.groupAmiChartDayCommand = new DelegateCommand<object>(this.CommandActionForDay, param => true));
            }
        }

        private void CommandActionForDay(object selected)
        {
            this.SelectedAMIsAction(selected, ResolutionType.DAY);
        }

        private void SelectedAMIsAction(object selectedTreeView, ResolutionType resolution)
        {
            object o = selectedTreeView;
            TreeView selected = (TreeView)selectedTreeView;
            TreeClasses selectedItem = (TreeClasses)selected.SelectedItem;
            Type t = selectedItem.GetType();

            switch (t.Name)
            {
                case "RootElement":
                    List<IdentifiedObject> amisC1 = this.Model.GetAllAmis();
                    List<long> ecsC1 = new List<long>();

                    foreach (EnergyConsumer ec in amisC1)
                    {
                        ecsC1.Add(ec.GlobalId);
                    }

                    ChartViewModel chartVM = new ChartViewModel() { Model = this.Model, Resolution = resolution };
                    chartVM.SetGids(ecsC1);

                    var doc = new List<DockWindowViewModel>();
                    doc.Add(new ChartViewModel() { Title = "All" });

                    this.DockManagerViewModel.Adding(doc);

                    //TabItems.Add(new TabItem()
                    //{
                    //    Header = "All",
                    //    CurrentTab = chartVM
                    //});

                    //SelectedTab = TabItems.Last();

                    break;
                case "GeoRegionForTree":
                    this.ChartViewForGeoRegion(resolution, ((GeoRegionForTree)selectedItem).GeoRegion.GlobalId, ((GeoRegionForTree)selectedItem).GeoRegion.Name);

                    break;
                case "SubGeoRegionForTree":
                    ChartViewForSubGeoRegion(resolution, ((SubGeoRegionForTree)selectedItem).SubGeoRegion.GlobalId, ((SubGeoRegionForTree)selectedItem).SubGeoRegion.Name);

                    break;
                case "SubstationForTree":
                    ChartViewForSubstation(resolution, ((SubstationForTree)selectedItem).Substation.GlobalId, ((SubstationForTree)selectedItem).Substation.Name);

                    break;
                default:
                    break;
            }
        }

        public void ChartViewForSubstation(ResolutionType resolution, long gid, string header)
        {
            List<IdentifiedObject> amisC4 = this.Model.GetSomeAmis(gid);
            List<long> ecsC4 = new List<long>();

            foreach (EnergyConsumer ec in amisC4)
            {
                ecsC4.Add(ec.GlobalId);
            }

            ChartViewModel chartVM4 = new ChartViewModel() { Model = this.Model, Resolution = resolution };
            chartVM4.SetGids(ecsC4);

            var doc = new List<DockWindowViewModel>();
            doc.Add(new ChartViewModel() { Title = header });

            this.DockManagerViewModel.Adding(doc);
            //TabItems.Add(new TabItem()
            //{
            //    Header = header,
            //    CurrentTab = chartVM4
            //});

            //SelectedTab = TabItems.Last();
        }

        public void ChartViewForSubGeoRegion(ResolutionType resolution, long gid, string header)
        {
            List<IdentifiedObject> substationsC3 = this.Model.GetSomeSubstations(gid, true);
            List<IdentifiedObject> amisC3 = new List<IdentifiedObject>();
            List<long> ecsC3 = new List<long>();

            foreach (Substation ss in substationsC3)
            {
                amisC3.AddRange(this.Model.GetSomeAmis(ss.GlobalId));
            }

            foreach (EnergyConsumer ec in amisC3)
            {
                ecsC3.Add(ec.GlobalId);
            }

            ChartViewModel chartVM3 = new ChartViewModel() { Model = this.Model, Resolution = resolution };
            chartVM3.SetGids(ecsC3);
            var doc = new List<DockWindowViewModel>();
            doc.Add(new ChartViewModel() { Title = header });

            this.DockManagerViewModel.Adding(doc);
            //TabItems.Add(new TabItem()
            //{
            //    Header = header,
            //    CurrentTab = chartVM3
            //});

            //SelectedTab = TabItems.Last();
        }

        public void ChartViewForGeoRegion(ResolutionType resolution, long gid, string header)
        {
            List<IdentifiedObject> subRegionsC2 = this.Model.GetSomeSubregions(gid, true);
            List<IdentifiedObject> substationsC2 = new List<IdentifiedObject>();
            List<IdentifiedObject> amisC2 = new List<IdentifiedObject>();
            List<long> ecsC2 = new List<long>();

            foreach (SubGeographicalRegion sgr in subRegionsC2)
            {
                substationsC2.AddRange(this.Model.GetSomeSubstations(sgr.GlobalId, true));
            }

            foreach (Substation ss in substationsC2)
            {
                amisC2.AddRange(this.Model.GetSomeAmis(ss.GlobalId));
            }

            foreach (EnergyConsumer ec in amisC2)
            {
                ecsC2.Add(ec.GlobalId);
            }

            ChartViewModel chartVM2 = new ChartViewModel() { Model = this.Model, Resolution = resolution };
            chartVM2.SetGids(ecsC2);

            var doc = new List<DockWindowViewModel>();
            doc.Add(new ChartViewModel() { Title = header });

            this.DockManagerViewModel.Adding(doc);

            //TabItems.Add(new TabItem()
            //{
            //    Header = header,
            //    CurrentTab = chartVM2
            //});

            //SelectedTab = TabItems.Last();
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

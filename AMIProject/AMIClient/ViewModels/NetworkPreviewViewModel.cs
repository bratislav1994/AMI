﻿using AMIClient.View;
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
        private bool rightClick;
        private ObservableCollection<TabItem> tabItems = new ObservableCollection<TabItem>();
        private object selectedTab;

        public NetworkPreviewViewModel()
        {
            rootElements = new ObservableCollection<RootElement>();
            RightClick = false;
            TabItems.Add(new TabItem()
            {
                Header = "Table",
                CurrentTab = DataGridViewModel.Instance,
                Exit = Visibility.Hidden
            });

            SelectedTab = TabItems.First();
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
        
        public object SelectedTab
        {
            get { return selectedTab; }
            set
            {
                selectedTab = value;
                this.RaisePropertyChanged("SelectedTab");
            }
        }
        
        public ObservableCollection<TabItem> TabItems
        {
            get
            {
                return tabItems;
            }

            set
            {
                if (tabItems == value)
                {
                    return;
                }

                tabItems = value;
                RaisePropertyChanged("TabItems");
            }
        }

        public bool RightClick
        {
            get
            {
                return rightClick;
            }

            set
            {
                rightClick = value;
            }
        }

        public void SetModel(Model model)
        {
            this.Model = model;
            RootElements.Add(new RootElement(this.model));
            this.model.SetRoot(rootElements[0]);
        }

        private ICommand closeTabCommand;

        public ICommand CloseTabCommand
        {
            get { return closeTabCommand ?? (closeTabCommand = new DelegateCommand<TabItem>((t) => { TabItems.Remove(t); })); }
        }

        public void SelectedAMIAction(object ami)
        {
            EnergyConsumer ec = (EnergyConsumer)ami;
            ChartViewModel chartVM = new ChartViewModel() { Model = this.Model };
            chartVM.SetGids(new List<long>() { ec.GlobalId });

            TabItems.Add(new TabItem()
            {
                Header = ec.Name,
                CurrentTab = chartVM
            });

            SelectedTab = TabItems.Last();
        }

        private ICommand groupAmiChartCommand;

        public ICommand GroupAMIChartCommand
        {
            get
            {
                return this.groupAmiChartCommand ?? (this.groupAmiChartCommand = new DelegateCommand<object>(this.SelectedAMIsAction, param => true));
            }
        }

        private void SelectedAMIsAction(object selectedTreeView)
        {
            object o = selectedTreeView;
            TreeView selected = (TreeView)selectedTreeView;
            TreeClasses selectedItem = (TreeClasses)selected.SelectedItem;
            Type t = selectedItem.GetType();

            switch(t.Name)
            {
                case "RootElement":
                    List<GeographicalRegion> geoRegionsC1 = this.Model.GetAllRegions(true);
                    List<SubGeographicalRegion> subGeoRegionsC1 = this.Model.GetAllSubRegions(true);
                    List<Substation> substationsC1 = this.Model.GetAllSubstations(true);
                    List<EnergyConsumerForTable> amisC1 = this.Model.GetAllAmis(true);
                    List<long> ecsC1 = new List<long>();

                    foreach (EnergyConsumerForTable ect in amisC1)
                    {
                        ecsC1.Add(ect.Ami.GlobalId);
                    }

                    //cvm.OpenWindow(ecsC1);
                    ChartViewModel chartVM = new ChartViewModel() { Model = this.Model };
                    chartVM.SetGids(ecsC1);
                    TabItems.Add(new TabItem()
                    {
                        Header = "All",
                        CurrentTab = chartVM
                    });

                    SelectedTab = TabItems.Last();

                    break;
                case "GeoRegionForTree":
                    List<SubGeographicalRegion> subRegionsC2 = this.Model.GetSomeSubregions(((GeoRegionForTree)selectedItem).GeoRegion.GlobalId, true);
                    List<Substation> substationsC2 = new List<Substation>();
                    List<EnergyConsumerForTable> amisC2 = new List<EnergyConsumerForTable>();
                    List<long> ecsC2 = new List<long>();
                    foreach(SubGeographicalRegion sgr in subRegionsC2)
                    {
                        substationsC2.AddRange(this.Model.GetSomeSubstations(sgr.GlobalId, true));
                    }
                    foreach(Substation ss in substationsC2)
                    {
                        amisC2.AddRange(this.Model.GetSomeAmis(ss.GlobalId, true));
                    }
                    foreach(EnergyConsumerForTable ect in amisC2)
                    {
                        ecsC2.Add(ect.Ami.GlobalId);
                    }
                    //cvm.OpenWindow(ecsC2);
                    ChartViewModel chartVM2 = new ChartViewModel() { Model = this.Model };
                    chartVM2.SetGids(ecsC2);
                    TabItems.Add(new TabItem()
                    {
                        Header = ((GeoRegionForTree)selectedItem).GeoRegion.Name,
                        CurrentTab = chartVM2
                    });

                    SelectedTab = TabItems.Last();

                    break;
                case "SubGeoRegionForTree":
                    List<Substation> substationsC3 = this.Model.GetSomeSubstations(((SubGeoRegionForTree)selectedItem).SubGeoRegion.GlobalId, true);
                    List<EnergyConsumerForTable> amisC3 = new List<EnergyConsumerForTable>();
                    List<long> ecsC3 = new List<long>();
                    foreach (Substation ss in substationsC3)
                    {
                        amisC3.AddRange(this.Model.GetSomeAmis(ss.GlobalId, true));
                    }
                    foreach (EnergyConsumerForTable ect in amisC3)
                    {
                        ecsC3.Add(ect.Ami.GlobalId);
                    }
                    //cvm.OpenWindow(ecsC3);
                    ChartViewModel chartVM3 = new ChartViewModel() { Model = this.Model };
                    chartVM3.SetGids(ecsC3);
                    TabItems.Add(new TabItem()
                    {
                        Header = ((SubGeoRegionForTree)selectedItem).SubGeoRegion.Name,
                        CurrentTab = chartVM3
                    });

                    SelectedTab = TabItems.Last();

                    break;
                case "SubstationForTree":
                    List<EnergyConsumerForTable> amisC4 = this.Model.GetSomeAmis(((SubstationForTree)selectedItem).Substation.GlobalId, true);
                    List<long> ecsC4 = new List<long>();
                    foreach (EnergyConsumerForTable ect in amisC4)
                    {
                        ecsC4.Add(ect.Ami.GlobalId);
                    }
                    //cvm.OpenWindow(ecsC4);
                    ChartViewModel chartVM4 = new ChartViewModel() { Model = this.Model };
                    chartVM4.SetGids(ecsC4);
                    TabItems.Add(new TabItem()
                    {
                        Header = ((SubstationForTree)selectedItem).Substation.Name,
                        CurrentTab = chartVM4
                    });

                    SelectedTab = TabItems.Last();

                    break;
                default:
                    break;
            }
        }

        public void RightClickOn()
        {
            this.RightClick = true;
        }

        public void RightClickOff()
        {
            this.RightClick = false;
        }

        public bool IsRightClick()
        {
            return RightClick;
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

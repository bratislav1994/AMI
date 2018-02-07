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
    public class NetworkPreviewViewModel
    {
        private Model model;
        private ObservableCollection<RootElement> rootElements;
        private static NetworkPreviewViewModel instance;
        public DockManagerViewModel DockManagerViewModel { get; private set; }

        public NetworkPreviewViewModel()
        {
            rootElements = new ObservableCollection<RootElement>();
            DataGridViewModel.Instance.Title = "Table";
            var documents = new List<DockWindowViewModel>();
            documents.Add(DataGridViewModel.Instance);
            this.DockManagerViewModel = new DockManagerViewModel(documents);
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
            ChartViewModel chartVM = new ChartViewModel() { Model = this.Model, Resolution = resolution, Title = ec.Name };
            chartVM.SetGids(new List<long>() { ec.GlobalId });
            var doc = new List<DockWindowViewModel>();
            doc.Add(chartVM);
            this.DockManagerViewModel.Adding(doc);
        }

        public void ChartViewForSubstation(ResolutionType resolution, long gid, string header)
        {
            List<IdentifiedObject> amisC4 = this.Model.GetSomeAmis(gid);
            List<long> ecsC4 = new List<long>();

            foreach (EnergyConsumer ec in amisC4)
            {
                ecsC4.Add(ec.GlobalId);
            }

            ChartViewModel chartVM4 = new ChartViewModel() { Model = this.Model, Resolution = resolution, Title = header };
            chartVM4.SetGids(ecsC4);
            var doc = new List<DockWindowViewModel>();
            doc.Add(chartVM4);
            this.DockManagerViewModel.Adding(doc);
        }

        public void ChartViewForSubGeoRegion(ResolutionType resolution, long gid, string header)
        {
            List<IdentifiedObject> substationsC3 = this.Model.GetSomeSubstations(gid);
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

            ChartViewModel chartVM3 = new ChartViewModel() { Model = this.Model, Resolution = resolution, Title = header };
            chartVM3.SetGids(ecsC3);
            var doc = new List<DockWindowViewModel>();
            doc.Add(chartVM3);
            this.DockManagerViewModel.Adding(doc);
        }

        public void ChartViewForGeoRegion(ResolutionType resolution, long gid, string header)
        {
            List<IdentifiedObject> subRegionsC2 = this.Model.GetSomeSubregions(gid);
            List<IdentifiedObject> substationsC2 = new List<IdentifiedObject>();
            List<IdentifiedObject> amisC2 = new List<IdentifiedObject>();
            List<long> ecsC2 = new List<long>();

            foreach (SubGeographicalRegion sgr in subRegionsC2)
            {
                substationsC2.AddRange(this.Model.GetSomeSubstations(sgr.GlobalId));
            }

            foreach (Substation ss in substationsC2)
            {
                amisC2.AddRange(this.Model.GetSomeAmis(ss.GlobalId));
            }

            foreach (EnergyConsumer ec in amisC2)
            {
                ecsC2.Add(ec.GlobalId);
            }

            ChartViewModel chartVM2 = new ChartViewModel() { Model = this.Model, Resolution = resolution, Title = header };
            chartVM2.SetGids(ecsC2);
            var doc = new List<DockWindowViewModel>();
            doc.Add(chartVM2);
            this.DockManagerViewModel.Adding(doc);
        }

        public void ConsumptionStatisticForAMI(object ami)
        {
            IdentifiedObject ec = (IdentifiedObject)ami;

            ConsumptionStatisticViewModel consumptiontVM = new ConsumptionStatisticViewModel()
            {
                Model = this.Model,
                Title = ec.Name,
                IsConsumerTypeCheckBoxEnabled = false,
                ConsumerTypeCb = new List<ConsumerType>() { ((EnergyConsumer)ec).Type }
            };

            consumptiontVM.SetGids(new List<long>() { ec.GlobalId });
            var doc = new List<DockWindowViewModel>();
            doc.Add(consumptiontVM);
            this.DockManagerViewModel.Adding(doc);
        }

        public void ConsumptionStatisticForSubstation(long gid, string header)
        {
            List<IdentifiedObject> amisC4 = this.Model.GetSomeAmis(gid);
            List<long> ecsC4 = new List<long>();
            List<ConsumerType> consumerTypes = new List<ConsumerType>();

            foreach (EnergyConsumer ec in amisC4)
            {
                ecsC4.Add(ec.GlobalId);

                if (!consumerTypes.Contains(ec.Type))
                {
                    consumerTypes.Add(ec.Type);
                }
            }

            ConsumptionStatisticViewModel consumptiontVM4 = new ConsumptionStatisticViewModel()
            {
                Model = this.Model,
                Title = header,
                IsConsumerTypeCheckBoxEnabled = true,
                ConsumerTypeCb = consumerTypes
            };

            consumptiontVM4.SetGids(ecsC4);
            var doc = new List<DockWindowViewModel>();
            doc.Add(consumptiontVM4);
            this.DockManagerViewModel.Adding(doc);
        }

        public void ConsumptionStatisticForSubGeoRegion(long gid, string header)
        {
            List<IdentifiedObject> substationsC3 = this.Model.GetSomeSubstations(gid);
            List<IdentifiedObject> amisC3 = new List<IdentifiedObject>();
            List<long> ecsC3 = new List<long>();

            foreach (Substation ss in substationsC3)
            {
                amisC3.AddRange(this.Model.GetSomeAmis(ss.GlobalId));
            }

            List<ConsumerType> consumerTypes = new List<ConsumerType>();

            foreach (EnergyConsumer ec in amisC3)
            {
                ecsC3.Add(ec.GlobalId);

                if (!consumerTypes.Contains(ec.Type))
                {
                    consumerTypes.Add(ec.Type);
                }
            }

            ConsumptionStatisticViewModel consumptiontVM3 = new ConsumptionStatisticViewModel()
            {
                Model = this.Model,
                Title = header,
                IsConsumerTypeCheckBoxEnabled = true,
                ConsumerTypeCb = consumerTypes
            };

            consumptiontVM3.SetGids(ecsC3);
            var doc = new List<DockWindowViewModel>();
            doc.Add(consumptiontVM3);
            this.DockManagerViewModel.Adding(doc);
        }

        public void ConsumptionStatisticForGeoRegion(long gid, string header)
        {
            List<IdentifiedObject> subRegionsC2 = this.Model.GetSomeSubregions(gid);
            List<IdentifiedObject> substationsC2 = new List<IdentifiedObject>();
            List<IdentifiedObject> amisC2 = new List<IdentifiedObject>();
            List<long> ecsC2 = new List<long>();

            foreach (SubGeographicalRegion sgr in subRegionsC2)
            {
                substationsC2.AddRange(this.Model.GetSomeSubstations(sgr.GlobalId));
            }

            foreach (Substation ss in substationsC2)
            {
                amisC2.AddRange(this.Model.GetSomeAmis(ss.GlobalId));
            }

            List<ConsumerType> consumerTypes = new List<ConsumerType>();

            foreach (EnergyConsumer ec in amisC2)
            {
                ecsC2.Add(ec.GlobalId);

                if (!consumerTypes.Contains(ec.Type))
                {
                    consumerTypes.Add(ec.Type);
                }
            }
            
            ConsumptionStatisticViewModel consumptiontVM2 = new ConsumptionStatisticViewModel()
            {
                Model = this.Model,
                Title = header,
                IsConsumerTypeCheckBoxEnabled = true,
                ConsumerTypeCb = consumerTypes
            };

            consumptiontVM2.SetGids(ecsC2);
            var doc = new List<DockWindowViewModel>();
            doc.Add(consumptiontVM2);
            this.DockManagerViewModel.Adding(doc);
        }
    }
}

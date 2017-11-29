﻿using FTN.Common;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class AMIsViewModel : INotifyPropertyChanged
    {
        private object substation;
        private object subGeoRegion;
        private object geoRegion;
        private ObservableCollection<EnergyConsumer> amis;
        private ObservableCollection<object> geoRegions;
        private ObservableCollection<object> subGeoRegions;
        private ObservableCollection<object> substations;
        private IModel model;

        public AMIsViewModel(IModel model)
        {
            this.model = model;
            Amis = new ObservableCollection<EnergyConsumer>();
            Substations = new ObservableCollection<object>() { "All" };
            GeoRegions = new ObservableCollection<object>() { "All" };
            SubGeoRegions = new ObservableCollection<object>() { "All" };
            GeoRegion = GeoRegions[0];
            SubGeoRegion = SubGeoRegions[0];
            GeoRegions.AddRange(this.model.GetAllRegions());
            SubGeoRegions.AddRange(this.model.GetAllSubRegions());
            Substations.AddRange(this.model.GetAllSubstations());
            Substation = Substations[0];
            Amis.AddRange(this.model.GetAllAmis());
        }

        public object Substation
        {
            get
            {
                return substation;
            }

            set
            {
                substation = value;
                RaisePropertyChanged("Substation");
            }
        }

        public object SubGeoRegion
        {
            get
            {
                return subGeoRegion;
            }

            set
            {
                subGeoRegion = value;
                RaisePropertyChanged("SubGeoRegion");

                Substations = new ObservableCollection<object>() { substations.First() };
                Substation = substations[0];

                if (SubGeoRegion != null)
                {
                    if (SubGeoRegion.Equals("All"))
                    {
                        for (int i = 1; i < SubGeoRegions.Count; i++)
                        {
                            Substations.AddRange(this.model.GetSomeSubstations(((SubGeographicalRegion)SubGeoRegions[i]).GlobalId));
                        }
                    }
                    else
                    {
                        Substations.AddRange(this.model.GetSomeSubstations(((SubGeographicalRegion)SubGeoRegion).GlobalId));
                    }
                }
            }
        }

        public object GeoRegion
        {
            get
            {
                return geoRegion;
            }

            set
            {
                geoRegion = value;
                RaisePropertyChanged("GeoRegion");

                SubGeoRegions = new ObservableCollection<object>() { subGeoRegions.First() };
                SubGeoRegion = subGeoRegions[0];
                Substations = new ObservableCollection<object>() { substations.First() };
                Substation = substations[0];

                if (GeoRegion.Equals("All"))
                {
                    SubGeoRegions.AddRange(this.model.GetAllSubRegions());
                    Substations.AddRange(this.model.GetAllSubstations());
                }
                else
                {
                    SubGeoRegions.AddRange(this.model.GetSomeSubregions(((GeographicalRegion)geoRegion).GlobalId));
                    for (int i = 1; i < SubGeoRegions.Count; i++)
                    {
                        Substations.AddRange(this.model.GetSomeSubstations(((SubGeographicalRegion)SubGeoRegions[i]).GlobalId));
                    }
                }

            }
        }

        public ObservableCollection<EnergyConsumer> Amis
        {
            get
            {
                return amis;
            }

            set
            {
                amis = value;
                RaisePropertyChanged("Amis");
            }
        }

        public ObservableCollection<object> GeoRegions
        {
            get
            {
                return geoRegions;
            }

            set
            {
                geoRegions = value;
                RaisePropertyChanged("GeoRegions");
            }
        }

        public ObservableCollection<object> SubGeoRegions
        {
            get
            {
                return subGeoRegions;
            }

            set
            {
                subGeoRegions = value;
                RaisePropertyChanged("SubGeoRegions");
            }
        }

        public ObservableCollection<object> Substations
        {
            get
            {
                return substations;
            }

            set
            {
                substations = value;
                RaisePropertyChanged("Substations");
            }
        }

        private DelegateCommand getElementsCommand;
        public DelegateCommand GetElementsCommand
        {
            get
            {
                if (getElementsCommand == null)
                {
                    getElementsCommand = new DelegateCommand(GetElementsCommandAction);
                }

                return getElementsCommand;
            }
            set
            {
                getElementsCommand = value;
            }
        }

        private void GetElementsCommandAction()
        {
            Amis.Clear();

            if (!substation.Equals("All"))
            {
                Amis.AddRange(this.model.GetSomeAmis(((Substation)Substation).GlobalId));
            }
            else if (Substation.Equals("All") && !SubGeoRegion.Equals("All"))
            {
                for (int i = 1; i < Substations.Count; i++)
                {
                    Amis.AddRange(this.model.GetSomeAmis(((Substation)Substations[i]).GlobalId));
                }
            }
            else if (Substation.Equals("All") && SubGeoRegion.Equals("All") && !GeoRegion.Equals("All"))
            {
                for (int i = 1; i < Substations.Count; i++)
                {
                    Amis.AddRange(this.model.GetSomeAmis(((Substation)Substations[i]).GlobalId));
                }
            }
            else
            {
                Amis.AddRange(this.model.GetAllAmis());
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
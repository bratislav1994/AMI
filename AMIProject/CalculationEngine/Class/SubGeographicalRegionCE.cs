﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace CalculationEngine.Class
{
    [DataContract]
    public class SubGeographicalRegionCE
    {
        private long globalId;
        private string mRID;
        private string name;
        private long geoRegionId;
        private GeographicalRegionCE geoRegionCE;
        private List<long> substations = new List<long>();
        private List<SubstationCE> substationsCE = new List<SubstationCE>();

        public SubGeographicalRegionCE()
        {
        }

        public SubGeographicalRegionCE(SubGeographicalRegion subGeoRegion)
        {
            this.globalId = subGeoRegion.GlobalId;
            this.mRID = subGeoRegion.Mrid;
            this.name = subGeoRegion.Name;
            this.geoRegionId = subGeoRegion.GeoRegion;
            this.substations = subGeoRegion.Substations;
        }

        [DataMember]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GlobalId
        {
            get
            {
                return globalId;
            }

            set
            {
                globalId = value;
            }
        }

        [DataMember]
        public string MRID
        {
            get
            {
                return mRID;
            }

            set
            {
                mRID = value;
            }
        }

        [DataMember]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        [DataMember]
        [ForeignKey("GeoRegionCE")]
        public long GeoRegionID
        {
            get
            {
                return geoRegionId;
            }

            set
            {
                geoRegionId = value;
            }
        }

        public GeographicalRegionCE GeoRegionCE
        {
            get { return geoRegionCE; }
            set { geoRegionCE = value; }
        }

        public List<long> Substations
        {
            get
            {
                return substations;
            }

            set
            {
                substations = value;
            }
        }
    }
}

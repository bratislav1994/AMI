using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace FTN.Services.NetworkModelService.DataModel
{
    [DataContract]
    public class GeographicalRegionDb
    {
        private long globalId;
        private string mRID;
        private string name;
        private List<long> subGeoRegion = new List<long>();
        private List<SubGeographicalRegionDb> subGeoRegionsCE = new List<SubGeographicalRegionDb>();

        public GeographicalRegionDb()
        {
        }

        public GeographicalRegionDb(GeographicalRegion geoRegion)
        {
            this.globalId = geoRegion.GlobalId;
            this.mRID = geoRegion.Mrid;
            this.name = geoRegion.Name;
            this.subGeoRegion = geoRegion.SubGeoRegions;
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

        public List<long> SubGeoRegion
        {
            get
            {
                return subGeoRegion;
            }

            set
            {
                subGeoRegion = value;
            }
        }
    }
}

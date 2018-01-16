using System;
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
    public class SubstationCE
    {
        private long globalId;
        private string mRID;
        private string name;
        private long subGeoRegionId;
        private SubGeographicalRegionCE subGeoRegionCE;
        private List<long> equipments = new List<long>();
        private List<EnergyConsumerCE> equipmentsCE = new List<EnergyConsumerCE>();

        public SubstationCE()
        {
        }

        public SubstationCE(Substation sub)
        {
            this.globalId = sub.GlobalId;
            this.mRID = sub.Mrid;
            this.name = sub.Name;
            this.subGeoRegionId = sub.SubGeoRegion;
            this.equipments = sub.Equipments;
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
        [ForeignKey("SubGeoRegionCE")]
        public long SubGeoRegionID
        {
            get
            {
                return subGeoRegionId;
            }

            set
            {
                subGeoRegionId = value;
            }
        }

        public SubGeographicalRegionCE SubGeoRegionCE
        {
            get
            {
                return subGeoRegionCE;
            }

            set
            {
                subGeoRegionCE = value;
            }
        }

        public List<long> Equipments
        {
            get
            {
                return equipments;
            }

            set
            {
                equipments = value;
            }
        }
    }
}

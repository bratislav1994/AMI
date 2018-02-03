using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Wires;

namespace FTN.Services.NetworkModelService.DataModel.Dynamic
{
    public class PowerTransformerForScada : DataForScada
    {
        private long globalId;
        private float invalidRangePercent;
        private float validRangePercent;
        private long baseVoltageId;
        private BaseVoltageForScada baseVoltage;
        private long substationId;
        private SubstationForScada substation;
        private string mRID;
        private string name;

        public PowerTransformerForScada()
        {

        }

        public PowerTransformerForScada(PowerTransformer pt)
        {
            this.globalId = pt.GlobalId;
            this.invalidRangePercent = pt.InvalidRangePercent;
            this.validRangePercent = pt.ValidRangePercent;
            this.baseVoltageId = pt.BaseVoltage;
            this.substationId = pt.EqContainer;
            this.mRID = pt.Mrid;
            this.name = pt.Name;
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
        public float InvalidRangePercent
        {
            get
            {
                return invalidRangePercent;
            }

            set
            {
                invalidRangePercent = value;
            }
        }

        [DataMember]
        public float ValidRangePercent
        {
            get
            {
                return validRangePercent;
            }

            set
            {
                validRangePercent = value;
            }
        }

        [DataMember]
        [ForeignKey("BaseVoltage")]
        public long BaseVoltageId
        {
            get
            {
                return baseVoltageId;
            }

            set
            {
                baseVoltageId = value;
            }
        }

        [IgnoreDataMember]
        public BaseVoltageForScada BaseVoltage
        {
            get
            {
                return baseVoltage;
            }

            set
            {
                baseVoltage = value;
            }
        }

        [DataMember]
        [ForeignKey("Substation")]
        public long SubstationId
        {
            get
            {
                return substationId;
            }

            set
            {
                substationId = value;
            }
        }

        [IgnoreDataMember]
        public SubstationForScada Substation
        {
            get
            {
                return substation;
            }

            set
            {
                substation = value;
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
    }
}

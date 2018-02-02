using FTN.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Wires;

namespace FTN.Services.NetworkModelService.DataModel
{
    [DataContract]
    public class EnergyConsumerDb
    {
        private long globalId;
        private string mRID;
        private string name;
        private float pMax;
        private float qMax;
        private float validRangePercent;
        private float invalidRangePercent;
        private long baseVoltage;
        private long eqContainerId;
        private ConsumerType type;
        private SubstationDb eqContainerCE;

        public EnergyConsumerDb()
        {
        }

        public EnergyConsumerDb(EnergyConsumer consumer)
        {
            this.globalId = consumer.GlobalId;
            this.mRID = consumer.Mrid;
            this.name = consumer.Name;
            this.pMax = consumer.PMax;
            this.qMax = consumer.QMax;
            this.ValidRangePercent = consumer.ValidRangePercent;
            this.InvalidRangePercent = consumer.InvalidRangePercent;
            this.baseVoltage = consumer.BaseVoltage;
            this.eqContainerId = consumer.EqContainer;
            this.type = consumer.Type;
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
        public float PMax
        {
            get
            {
                return pMax;
            }

            set
            {
                pMax = value;
            }
        }

        [DataMember]
        public float QMax
        {
            get
            {
                return qMax;
            }

            set
            {
                qMax = value;
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
        public long BaseVoltage
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
        [ForeignKey("EqContainerCE")]
        public long EqContainerID
        {
            get
            {
                return eqContainerId;
            }

            set
            {
                eqContainerId = value;
            }
        }

        public SubstationDb EqContainerCE
        {
            get
            {
                return eqContainerCE;
            }

            set
            {
                eqContainerCE = value;
            }
        }

        [DataMember]
        public ConsumerType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }
    }
}

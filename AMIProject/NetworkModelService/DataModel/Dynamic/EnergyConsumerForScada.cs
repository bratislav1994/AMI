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

namespace FTN.Services.NetworkModelService.DataModel.Dynamic
{
    [DataContract]
    public class EnergyConsumerForScada : DataForScada
    {
        private long globalId;
        private string mRID;
        private string name;
        private float pMax;
        private float qMax;
        private float validRangePercent;
        private float invalidRangePercent;
        private long baseVoltageId;
        private BaseVoltageForScada baseVoltage;
        private long eqContainerId;
        private SubstationForScada eqContainer;
        private ConsumerType type;

        public EnergyConsumerForScada()
        {
        }

        public EnergyConsumerForScada(EnergyConsumer consumer)
        {
            this.globalId = consumer.GlobalId;
            this.mRID = consumer.Mrid;
            this.name = consumer.Name;
            this.pMax = consumer.PMax;
            this.qMax = consumer.QMax;
            this.ValidRangePercent = consumer.ValidRangePercent;
            this.InvalidRangePercent = consumer.InvalidRangePercent;
            this.BaseVoltageId = consumer.BaseVoltage;
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
        [ForeignKey("EqContainer")]
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

        [IgnoreDataMember]
        public SubstationForScada EqContainer
        {
            get
            {
                return eqContainer;
            }

            set
            {
                eqContainer = value;
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

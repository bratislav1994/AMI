using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace FTN.Services.NetworkModelService.DataModel.Dynamic
{
    [DataContract]
    public class BaseVoltageDb
    {
        private long globalId;
        private string mRID;
        private string name;
        private float nominalVoltage;

        public BaseVoltageDb() { }

        public BaseVoltageDb(BaseVoltage bv)
        {
            this.GlobalId = bv.GlobalId;
            this.MRID = bv.Mrid;
            this.Name = bv.Name;
            this.NominalVoltage = bv.NominalVoltage;
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
        public float NominalVoltage
        {
            get
            {
                return nominalVoltage;
            }

            set
            {
                nominalVoltage = value;
            }
        }
    }
}

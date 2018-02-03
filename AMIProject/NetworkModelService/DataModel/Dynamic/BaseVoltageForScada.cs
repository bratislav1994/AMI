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
    public class BaseVoltageForScada : DataForScada
    {
        private long globalId;
        private float nominalVoltage;
        private string mRID;
        private string name;

        public BaseVoltageForScada()
        {

        }

        public BaseVoltageForScada(BaseVoltage bv)
        {
            this.globalId = bv.GlobalId;
            this.nominalVoltage = bv.NominalVoltage;
            this.mRID = bv.Mrid;
            this.name = bv.Name;
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

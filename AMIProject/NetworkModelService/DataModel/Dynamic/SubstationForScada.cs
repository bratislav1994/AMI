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
    public class SubstationForScada : DataForScada
    {
        private long globalId;
        private long subGeoRegion;
        private string mRID;
        private string name;

        public SubstationForScada()
        {

        }

        public SubstationForScada(Substation ss)
        {
            this.GlobalId = ss.GlobalId;
            this.MRID = ss.Mrid;
            this.SubGeoRegion = ss.SubGeoRegion;
            this.Name = ss.Name;
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
        public long SubGeoRegion
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

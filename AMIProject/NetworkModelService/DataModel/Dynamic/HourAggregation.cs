using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FTN.Services.NetworkModelService.DataModel.Dynamic
{
    public class HourAggregation : Statistics
    {
        private int idDbH;
        private long psrRef;

        public HourAggregation()
        {
            this.MinP = 0;
            this.MinQ = 0;
            this.MinV = 0;
            this.MaxP = 0;
            this.MaxQ = 0;
            this.MaxV = 0;
            this.AvgP = 0;
            this.AvgQ = 0;
            this.AvgV = 0;
            this.IntegralP = 0;
            this.IntegralQ = 0;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDB
        {
            get
            {
                return idDbH;
            }

            set
            {
                idDbH = value;
            }
        }

        public long PsrRef
        {
            get
            {
                return psrRef;
            }

            set
            {
                psrRef = value;
            }
        }
    }
}

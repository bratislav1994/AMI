using FTN.Common;
using SCADA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;

namespace SCADA
{
    public class MeasurementForScada
    {
        private int idDB;
        private int index;
        private int measurementId;
        private Measurement measurement;
        private int wrapperDbId;
        private WrapperDB wrapperDb;

        public MeasurementForScada()
        {
            wrapperDb = null;
        }

        public MeasurementForScada(int wrapperId)
        {
            this.wrapperDbId = wrapperId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDB
        {
            get { return idDB; }
            set { idDB = value; }
        }

        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
            }
        }

        public Measurement Measurement
        {
            get
            {
                return measurement;
            }

            set
            {
                measurement = value;
            }
        }

        [ForeignKey("Measurement")]
        public int MeasurementId
        {
            get
            {
                return measurementId;
            }

            set
            {
                measurementId = value;
            }
        }

        [ForeignKey("WrapperDb")]
        public int WrapperDbId
        {
            get
            {
                return wrapperDbId;
            }

            set
            {
                wrapperDbId = value;
            }
        }

        public WrapperDB WrapperDb
        {
            get
            {
                return wrapperDb;
            }

            set
            {
                wrapperDb = value;
            }
        }
    }
}

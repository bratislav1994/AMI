using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel
{
    public class WrapperDB
    {
        private int idDB;
        private int rtuAddress;
        private List<MeasurementForScada> listOfMeasurements;

        public WrapperDB()
        {
            listOfMeasurements = new List<MeasurementForScada>();
        }

        public WrapperDB(int rtuAddress)
        {
            this.rtuAddress = rtuAddress;
        }

        public WrapperDB(int rtuAddress, List<MeasurementForScada> listOfMeas)
        {
            this.rtuAddress = rtuAddress;
            this.listOfMeasurements = listOfMeas;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDB
        {
            get { return idDB; }
            set { idDB = value; }
        }

        public int RtuAddress
        {
            get
            {
                return rtuAddress;
            }

            set
            {
                rtuAddress = value;
            }
        }

        public List<MeasurementForScada> ListOfMeasurements
        {
            get
            {
                return listOfMeasurements;
            }

            set
            {
                listOfMeasurements = value;
            }
        }
    }
}

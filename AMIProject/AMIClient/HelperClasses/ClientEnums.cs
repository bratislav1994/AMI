using FTN.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClient.HelperClasses
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DataGridType : int
    {
        [Description("Energy consumer")]
        ENERGY_CONSUMER = 0,
        [Description("Geographical region")]
        GEOGRAPHICALREGION,
        [Description("SubGeographical region")]
        SUBGEOGRAPHICALREGION,
        [Description("Substation")]
        SUBSTATION
    }

    public enum DataGridHeader
    {
        Name = 1,
        Type
    }

    public enum DataGridAlarmHeader
    {
        Consumer = 1,
        Status,
        TypeVoltage
    }
}

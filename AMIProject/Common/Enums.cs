using System;
using System.ComponentModel;

namespace FTN.Common
{
    /// <summary>
    /// The units defiend for usage in the CIM.
    /// </summary>
    public enum UnitSymbol : int
    {

        P = 0,
        Q,
        V

    }//end UnitSymbol

    public enum Direction : int
    {
        READ = 0,
        READWRITE,
        WRITE
    }//end Direction

    public enum ConsumerType : int
    {
        FIRM = 0,
        HOUSEHOLD,
        SHOPPING_CENTER,
    }//end Direction

    public enum ResolutionType : int
    {
        MINUTE = 0,
        HOUR,
        DAY
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TypeVoltage : int
    {
        [Description("Under voltage")]
        UNDERVOLTAGE = 0,
        [Description("Over voltage")]
        OVERVOLTAGE,
        INBOUNDS
    }
}

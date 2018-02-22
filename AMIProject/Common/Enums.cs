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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ConsumerType : int
    {
        [Description("Firm")]
        FIRM = 0,
        [Description("Household")]
        HOUSEHOLD,
        [Description("Shopping center")]
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Season : int
    {
        [Description("Summer")]
        SUMMER = 0,
        [Description("Winter")]
        WINTER
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TypeOfDay : int
    {
        [Description("Work day")]
        WORKDAY = 0,
        [Description("Weekend")]
        WEEKEND
    }

    public enum ServiceType : int
    {
        STATEFFUL = 0,
        STATELESS
    }
}

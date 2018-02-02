using System;
using System.Collections.Generic;
using System.Text;

namespace FTN.Common
{
	
	public enum DMSType : short
	{		
		MASK_TYPE							= unchecked((short)0xFFFF),

        GEOREGION                           = 0x0001,
        SUBGEOREGION                        = 0x0002,
        BASEVOLTAGE                         = 0x0003,
        SUBSTATION                          = 0x0004,
        ENERGYCONS                          = 0x0005,
        POWERTRANSFORMER                    = 0x0006,
        ANALOG                              = 0x0007,   
        DISCRETE                            = 0x0008,   
	}

    [Flags]
	public enum ModelCode : long
	{
		IDOBJ								= 0x1000000000000000,
		IDOBJ_GID							= 0x1000000000000104,
		IDOBJ_MRID							= 0x1000000000000207,
        IDOBJ_NAME                          = 0x1000000000000307,

        MEASUREMENT                         = 0x1100000000000000,
        MEASUREMENT_UNITSYMBOL              = 0x110000000000010a,
        MEASUREMENT_DIRECTION               = 0x110000000000020a,
        MEASUREMENT_RTUADDRESS              = 0X1100000000000303,
        MEASUREMENT_PSR                     = 0x1100000000000409,

        BASEVOLTAGE                         = 0x1200000000030000,
        BASEVOLTAGE_NOMINALVOL              = 0x1200000000030105,
        BASEVOLTAGE_CONDEQS                 = 0x1200000000030219,

        PSR                                 = 0x1300000000000000,
        PSR_MEASUREMENTS                    = 0x1300000000000119,

        SUBGEOREGION                        = 0x1400000000020000,
        SUBGEOREGION_GEOREG                 = 0x1400000000020109,
        SUBGEOREGION_SUBS                   = 0x1400000000020219,

        GEOREGION                           = 0x1500000000010000,
        GEOREGION_SUBGEOREGIONS             = 0x1500000000010119,
        
        ANALOG                              = 0x1110000000070000,

        DISCRETE                            = 0x1120000000080000,
        DISCRETE_MAXVALUE                   = 0x1120000000080103,
        DISCRETE_MINVALUE                   = 0x1120000000080203,
        DISCRETE_NORMALVALUE                = 0x1120000000080303,

        EQUIPMENT                           = 0x1310000000000000,
        EQUIPMENT_EQCONTAINER               = 0x1310000000000109,
        
        CONNODECONTAINER                    = 0x1320000000000000,

        CONDEQ                              = 0x1311000000000000,
        CONDEQ_BASEVOLTAGE                  = 0x1311000000000109,

        EQCONTAINER                         = 0x1321000000000000,
        EQCONTAINER_EQUIPMENTS              = 0x1321000000000119,

        ENERGYCONS                          = 0x1311100000050000,
        ENERGYCONS_PMAX                     = 0x1311100000050105,
        ENERGYCONS_QMAX                     = 0x1311100000050205,
        ENERGYCONS_TYPE                     = 0x131110000005030a,
        ENERGYCONS_VALIDRANGEPERCENT        = 0x1311100000050405,
        ENERGYCONS_INVALIDRANGEPERCENT      = 0x1311100000050505,

        POWERTRANSFORMER                    = 0x1311200000060000,
        POWERTRANSFORMER_VALIDRANGEPERCENT  = 0x1311200000060105,
        POWERTRANSFORMER_INVALIDRANGEPERCENT= 0x1311200000060205,

        SUBSTATION                          = 0x1321100000040000,
        SUBSTATION_SUBGEOREGION             = 0x1321100000040109,
    }

    [Flags]
	public enum ModelCodeMask : long
	{
		MASK_TYPE			 = 0x00000000ffff0000,
		MASK_ATTRIBUTE_INDEX = 0x000000000000ff00,
		MASK_ATTRIBUTE_TYPE	 = 0x00000000000000ff,

		MASK_INHERITANCE_ONLY = unchecked((long)0xffffffff00000000),
		MASK_FIRSTNBL		  = unchecked((long)0xf000000000000000),
		MASK_DELFROMNBL8	  = unchecked((long)0xfffffff000000000),		
	}																		
}



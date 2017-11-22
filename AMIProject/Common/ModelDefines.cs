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
        VOLTAGELEVEL                        = 0x0005,
        ENERGYCONS                          = 0x0006,
        POWERTRANSFORMER                    = 0x0007,
        POWERTRANSEND                       = 0x0008,
        RATIOTAPCHANGER                     = 0x0009,
        ANALOG                              = 0x000a,   
        DISCRETE                            = 0x000b,   
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
        MEASUREMENT_PSR                     = 0x1100000000000209,

        BASEVOLTAGE                         = 0x1200000000030000,
        BASEVOLTAGE_NOMINALVOL              = 0x1200000000030105,
        BASEVOLTAGE_CONDEQS                 = 0x1200000000030219,
        BASEVOLTAGE_TRANSENDS               = 0x1200000000030319,
        BASEVOLTAGE_VOLTLEVELS              = 0x1200000000030419,

        TRANSFORMEREND                      = 0x1300000000000000,
        TRANSFORMEREND_BASEVOLT             = 0x1300000000000109,
        TRANSFORMEREND_RATIOTAPCHANGER      = 0x1300000000000219,

        PSR                                 = 0x1400000000000000,
        PSR_MEASUREMENTS                    = 0x1400000000000119,

        SUBGEOREGION                        = 0x1500000000020000,
        SUBGEOREGION_GEOREG                 = 0x1500000000020109,
        SUBGEOREGION_SUBS                   = 0x1500000000020219,

        GEOREGION                           = 0x1600000000010000,
        GEOREGION_SUBGEOREGIONS             = 0x1600000000010119,
        

        ANALOG                              = 0x11100000000a0000,
        ANALOG_MAXVALUE                     = 0x11100000000a0105,
        ANALOG_MINVALUE                     = 0x11100000000a0205,
        ANALOG_NORMALVALUE                  = 0x11100000000a0305,

        DISCRETE                            = 0x11200000000b0000,
        DISCRETE_MAXVALUE                   = 0x11200000000b0103,
        DISCRETE_MINVALUE                   = 0x11200000000b0203,
        DISCRETE_NORMALVALUE                = 0x11200000000b0303,

        POWERTRANSEND                       = 0x1310000000080000,
        POWERTRANSEND_POWERTRANSF           = 0x1310000000080109,

        EQUIPMENT                           = 0x1410000000000000,
        EQUIPMENT_EQCONTAINER               = 0x1410000000000109,

        TAPCHANGER                          = 0x1420000000000000,
        TAPCHANGER_HIGHSTEP                 = 0x1420000000000103,
        TAPCHANGER_LOWSTEP                  = 0x1420000000000203,
        TAPCHANGER_NEUTRALSTEP              = 0x1420000000000303,
        TAPCHANGER_NORMALSTEP               = 0x1420000000000403,

        CONNODECONTAINER                    = 0x1430000000000000,
        

        CONDEQ                              = 0x1411000000000000,
        CONDEQ_BASEVOLTAGE                  = 0x1411000000000109,

        EQCONTAINER                         = 0x1431000000000000,
        EQCONTAINER_EQUIPMENTS              = 0x1431000000000119,

        RATIOTAPCHANGER                     = 0x1421000000090000,
        RATIOTAPCHANGER_TRANSEND            = 0x1421000000090109,
        

        ENERGYCONS                          = 0x1411100000060000,
        ENERGYCONS_PFIXED                   = 0x1411100000060105,
        ENERGYCONS_QFIXED                   = 0x1411100000060205,

        POWERTRANSFORMER                    = 0x1411200000070000,
        POWERTRANSFORMER_POWTRANSENDS       = 0x1411200000070119,

        VOLTAGELEVEL                        = 0x1431100000050000,
        VOLTAGELEVEL_SUBSTATION             = 0x1431100000050109,
        VOLTAGELEVEL_BASEVOLTAGE            = 0x1431100000050209,

        SUBSTATION                          = 0x1431200000040000,
        SUBSTATION_SUBGEOREGION             = 0x1431200000040109,
        SUBSTATION_VOLTLEVELS               = 0x1431200000040219,
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



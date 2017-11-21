using System;
using System.Collections.Generic;
using System.Text;

namespace FTN.Common
{
	
	public enum DMSType : short
	{		
		MASK_TYPE							= unchecked((short)0xFFFF),

        REGULCONTROL                        = 0x0001,
        REACTCAPABCURVE                     = 0x0002,
        CURVEDATA                           = 0x0003,
        SYNMACHINE                          = 0x0004,
        FREQCONVERTER                       = 0x0005,
        SHUNTCOMPENSATOR                    = 0x0006,
        STATICVARCOMPENSATOR                = 0x0007,
        CONTROL                             = 0x0008,
        TERMINAL                            = 0x0009,
	}

    [Flags]
	public enum ModelCode : long
	{
		IDOBJ								= 0x1000000000000000,
		IDOBJ_GID							= 0x1000000000000104,
		IDOBJ_MRID							= 0x1000000000000207,
        IDOBJ_NAME                          = 0x00,

        MEASUREMENT                         = 0x00,
        MEASUREMENT_UNITSYMBOL              = 0x00,

        BASEVOLTAGE                         = 0x00,
        BASEVOLTAGE_NOMINALVOL              = 0x00,

        TRANSFORMEREND                      = 0x00,

        PSR                                 = 0x00,

        SUBGEOREGION                        = 0x00,

        GEOREGION                           = 0x00,

        //------------------------------------------------
        ANALOG                              = 0x00,
        ANALOG_MAXVALUE                     = 0x00,
        ANALOG_MINVALUE                     = 0x00,
        ANALOG_NORMALVALUE                  = 0x00,

        DISCRETE                            = 0x00,
        DISCRETE_MAXVALUE                   = 0x00,
        DISCRETE_MINVALUE                   = 0x00,
        DISCRETE_NORMALVALUE                = 0x00,

        POWERTRANSEND                       = 0x00,

        EQUIPMENT                           = 0x00,

        TAPCHANGER                          = 0x00,
        TAPCHANGER_HIGHSTEP                 = 0x00,
        TAPCHANGER_LOWSTEP                  = 0x00,
        TAPCHANGER_NEUTRALSTEP              = 0x00,
        TAPCHANGER_NORMALSTEP               = 0x00,

        CONNODECONTAINER                    = 0x00,

        //------------------------------------------------
        CONDEQ                              = 0x00,

        EQCONTAINER                         = 0x00,

        RATIOTAPCHANGER                     = 0x00,

        //-------------------------------------------------
        ENERGYCONS                          = 0x00,
        ENERGYCONS_PFIXED                   = 0x00,
        ENERGYCONS_QFIXED                   = 0x00,

        POWERTRANSFORMER                    = 0x00,

        VOLTAGELEVEL                        = 0x00,

        SUBSTATION                          = 0x00,



  //      CONTROL                             = 0x1100000000080000,
  //      CONTROL_REGULCONDEQ                 = 0x1100000000080109,

  //      TERMINAL                            = 0x1200000000090000,
  //      TERMINAL_CONDEQ                     = 0x1200000000090109,

		//PSR									= 0x1300000000000000,

  //      EQUIPMENT                           = 0x1310000000000000,
  //      EQUIPMENT_AGGREGATE                 = 0x1310000000000101,
  //      EQUIPMENT_NORMINSERVICE             = 0x1310000000000201,

  //      CONDEQ                              = 0x1311000000000000,
  //      CONDEQ_TERMINALS                    = 0x1311000000000119,

  //      REGULCONDEQ                         = 0x1311100000000000,
  //      REGULCONDEQ_CONTROLS                = 0x1311100000000119,
  //      REGULCONDEQ_REGULCONTROL            = 0x1311100000000209,

  //      ROTATINGMACHINE                     = 0x1311110000000000,

  //      SYNMACHINE                          = 0x1311111000040000,
  //      SYNMACHINE_REACTCAPABCURVE          = 0x1311111000040109,

  //      FREQCONVERTER                       = 0x1311120000050000,

  //      SHUNTCOMPENSATOR                    = 0x1311130000060000,

  //      STATICVARCOMPENSATOR                = 0x1311140000070000,

  //      REGULCONTROL                        = 0x1320000000010000,
  //      REGULCONTROL_DISCRETE               = 0x1320000000010101,
  //      REGULCONTROL_MODE                   = 0x132000000001020a,
  //      REGULCONTROL_MONITPHASE             = 0x132000000001030a,
  //      REGULCONTROL_TARGETRANGE            = 0x1320000000010405,
  //      REGULCONTROL_TARGETVALUE            = 0x1320000000010505,
  //      REGULCONTROL_REGULCONDEQS           = 0x1320000000010619,

  //      CURVE                               = 0x1400000000000000,
  //      CURVE_CURVESTYLE                    = 0x140000000000010a,
  //      CURVE_XMYLTIPLIER                   = 0x140000000000020a,
  //      CURVE_XUNIT                         = 0x140000000000030a,
  //      CURVE_Y1MULTIPLIER                  = 0x140000000000040a,
  //      CURVE_Y1UNIT                        = 0x140000000000050a,
  //      CURVE_Y2MULTIPLIER                  = 0x140000000000060a,
  //      CURVE_Y2UNIT                        = 0x140000000000070a,
  //      CURVE_Y3MULTIPLIER                  = 0x140000000000080a,
  //      CURVE_Y3UNIT                        = 0x140000000000090a,
  //      CURVE_CURVEDATAS                    = 0x1400000000000a19,

  //      REACTCAPABCURVE                     = 0x1410000000020000,
  //      REACTCAPABCURVE_SYNMACHINES         = 0x1410000000020119,

  //      CURVEDATA                           = 0x1500000000030000,
  //      CURVEDATA_XVALUE                    = 0x1500000000030105,
  //      CURVEDATA_Y1VALUE                   = 0x1500000000030205,
  //      CURVEDATA_Y2VALUE                   = 0x1500000000030305,
  //      CURVEDATA_Y3VALUE                   = 0x1500000000030405,
  //      CURVEDATA_CURVE                     = 0x1500000000030509,
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



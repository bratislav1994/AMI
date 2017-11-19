using System;

namespace FTN.Common
{	
    public enum CurveStyle : short
    {
        constantYValue = 0,
        formula = 1,
        rampYValue = 2,
        straightLineYValues = 3
    }
    
	public enum PhaseCode : short
	{
		Unknown = 0x0,
		N = 0x1,
		C = 0x2,
		CN = 0x3,
		B = 0x4,
		BN = 0x5,
		BC = 0x6,
		BCN = 0x7,
		A = 0x8,
		AN = 0x9,
		AC = 0xA,
		ACN = 0xB,
		AB = 0xC,
		ABN = 0xD,
		ABC = 0xE,
		ABCN = 0xF
	}

    public enum RegulatingControlModelKind : short
    {
        activePower = 0,
        admittance = 1,
        currentFlow = 2,
        @fixed = 3,
        powerFactor = 4,
        reactivePower = 5,
        temperature = 6,
        timeScheduled = 7,
        voltage = 8
    }

    public enum UnitMultiplier : short
    {
        c = 0,
        d = 1,
        G = 2,
        k = 3,
        m = 4,
        M = 5,
        micro = 6,
        n = 7,
        none = 8,
        P = 9,
        T = 10
    }

    public enum UnitSymbol : short
    {
        A = 0,
        deg = 1,
        degC = 2,
        F = 3,
        g = 4,
        h = 5,
        H = 6,
        Hz = 7,
        J = 8,
        m = 9,
        m2 = 10,
        m3 = 11,
        min = 12,
        N = 13,
        none = 14,
        ohm = 15,
        Pa = 16,
        rad = 17,
        s = 18,
        S = 19,
        V = 20,
        VA = 21,
        VAh = 22,
        VAr = 23,
        VArh = 24,
        W = 25,
        Wh = 26
    }
}

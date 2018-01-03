using System;

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
        WRITE = 0,
        READWRITE,
        READ
    }//end Direction

    public enum OperationType : int
    {
        INSERT = 0,
        UPDATE,
        DELETE
    }
}

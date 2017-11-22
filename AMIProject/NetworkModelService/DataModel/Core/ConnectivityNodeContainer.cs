///////////////////////////////////////////////////////////
//  ConnectivityNodeContainer.cs
//  Implementation of the Class ConnectivityNodeContainer
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:16:51 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



using TC57CIM.IEC61970.Core;
using FTN.Common;

namespace TC57CIM.IEC61970.Core {
	/// <summary>
	/// A base class for all objects that may contain connectivity nodes or topological
	/// nodes.
	/// </summary>
	public class ConnectivityNodeContainer : PowerSystemResource {
        
        public ConnectivityNodeContainer(long globalId) : base(globalId)
        {
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

    }//end ConnectivityNodeContainer

}//end namespace Core
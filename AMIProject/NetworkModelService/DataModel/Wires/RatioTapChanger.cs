///////////////////////////////////////////////////////////
//  RatioTapChanger.cs
//  Implementation of the Class RatioTapChanger
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:17:08 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



using TC57CIM.IEC61970.Wires;
namespace TC57CIM.IEC61970.Wires {
	/// <summary>
	/// A tap changer that changes the voltage ratio impacting the voltage magnitude
	/// but not the phase angle across the transformer.
	/// </summary>
	public class RatioTapChanger : TapChanger {

		/// <summary>
		/// The tap ratio table for this ratio  tap changer.
		/// </summary>
		public TC57CIM.IEC61970.Wires.RatioTapChangerTabular RatioTapChangerTabular;

		public RatioTapChanger(){

		}

		~RatioTapChanger(){

		}

	}//end RatioTapChanger

}//end namespace Wires
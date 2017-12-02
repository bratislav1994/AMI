using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using FTN.Common;


namespace FTN.ServiceContracts
{
	[ServiceContract]
	public interface INetworkModelGDAContract
	{
		/// <summary>
		/// Updates model by appluing reosoreces sent in delta
		/// </summary>		
		/// <param name="delta">Object which contains model changes</param>		
		/// <returns>Result of model changes</returns>
		[OperationContract]	
		UpdateResult ApplyUpdate(Delta delta);
	}
}

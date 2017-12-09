﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using FTN.Services.NetworkModelService;
using System.ServiceModel.Description;
using FTN.Common;
using FTN.ServiceContracts;

namespace FTN.Services.NetworkModelService
{
	public class NetworkModelService : IDisposable
	{				
		private NetworkModel nm = null;
		private List<ServiceHost> hosts = null;
        private ServiceHost svcDuplexClient = null;
        private ServiceHost svcDuplexScada = null;
        private ServiceHost svc = null;

        public NetworkModelService()
		{			
			nm = new NetworkModel();			
			GenericDataAccess.NetworkModel = nm;
            ResourceIterator.NetworkModel = nm;
            svcDuplexClient = new ServiceHost(typeof(GenericDataAccess));
            svcDuplexClient.AddServiceEndpoint(typeof(INetworkModelGDAContractDuplexClient), 
                                    new NetTcpBinding(),
                                    new Uri("net.tcp://localhost:10000/NetworkModelService/GDADuplexClient"));

            
            //InitializeHosts();
        }
	
		public void Start()
		{
            svcDuplexClient.Open();
            Console.WriteLine("WCF services opened and ready");
			//StartHosts();			
		}

		public void Dispose()
		{
            //CloseHosts();
            svcDuplexClient.Close();
            GC.SuppressFinalize(this);
		}

		private void InitializeHosts()
		{
			hosts = new List<ServiceHost>();
			hosts.Add(new ServiceHost(typeof(GenericDataAccess)));
		}

		private void StartHosts()
		{
			if (hosts == null || hosts.Count == 0)
			{
				throw new Exception("Network Model Services can not be opend because it is not initialized.");
			}

			string message = string.Empty;
			foreach (ServiceHost host in hosts)
			{
				host.Open();

				message = string.Format("The WCF service {0} is ready.", host.Description.Name);
				Console.WriteLine(message);
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

				message = "Endpoints:";
				Console.WriteLine(message);
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

				foreach (Uri uri in host.BaseAddresses)
				{
					Console.WriteLine(uri);
					CommonTrace.WriteTrace(CommonTrace.TraceInfo, uri.ToString());
				}

				Console.WriteLine("\n");
			}

			message = string.Format("Connection string: {0}", Config.Instance.ConnectionString);
			Console.WriteLine(message);
			CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

			message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
			Console.WriteLine(message);
			CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
			

			message = "The Network Model Service is started.";
			Console.WriteLine("\n{0}", message);
			CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
		}

		public void CloseHosts()
		{
			if (hosts == null || hosts.Count == 0)
			{
				throw new Exception("Network Model Services can not be closed because it is not initialized.");
			}

			foreach (ServiceHost host in hosts)
			{
				host.Close();
			}

			string message = "The Network Model Service is closed.";
			CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
			Console.WriteLine("\n\n{0}", message);
		}	
	}
}

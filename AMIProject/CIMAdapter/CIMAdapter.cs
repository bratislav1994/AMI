﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using CIM.Model;
using CIMParser;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Importer;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using FTN.ServiceContracts;
using System.ServiceModel;

namespace FTN.ESI.SIMES.CIM.CIMAdapter
{
	public class CIMAdapter
	{
        private ITransactionCoordinator proxy = null;
        private bool firstContact = true;
       
		public CIMAdapter()
		{
		}

        public ITransactionCoordinator Proxy
        {
            get
            {
                if (firstContact)
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.Transport;
                    binding.ReceiveTimeout = TimeSpan.MaxValue;
                    binding.SendTimeout = TimeSpan.FromHours(1);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

                    ChannelFactory<ITransactionCoordinator> factory = new ChannelFactory<ITransactionCoordinator>(
                        binding,
                         new EndpointAddress("net.tcp://finalamicluster.westus.cloudapp.azure.com:10300/TransactionCoordinatorProxy/Adapter/"));

                    factory.Credentials.Windows.ClientCredential.UserName = "amiteam";
                    factory.Credentials.Windows.ClientCredential.Password = "dr34mt34m4m1@";
                    factory.Credentials.UserName.UserName = "amiteam";
                    factory.Credentials.UserName.Password = "dr34mt34m4m1@";

                    this.proxy = factory.CreateChannel();
                    firstContact = false;
                }

                return proxy;
            }
        }
        
		public Delta CreateDelta(Stream extract, SupportedProfiles extractType, out string log)
		{
			Delta nmsDelta = null;
			ConcreteModel concreteModel = null;
			Assembly assembly = null;
			string loadLog = string.Empty;
			string transformLog = string.Empty;

			if (LoadModelFromExtractFile(extract, extractType, ref concreteModel, ref assembly, out loadLog))
			{
				DoTransformAndLoad(assembly, concreteModel, extractType, out nmsDelta, out transformLog);
			}
			log = string.Concat("Load report:\r\n", loadLog, "\r\nTransform report:\r\n", transformLog);

			return nmsDelta;
		}

		public string ApplyUpdates(Delta delta)
		{
			string updateResult = "Apply Updates Report:\r\n";
			System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			if ((delta != null) && (delta.NumberOfOperations != 0))
			{
				//// NetworkModelService->ApplyUpdates
                updateResult = Proxy.ApplyDelta(delta);
			}

			Thread.CurrentThread.CurrentCulture = culture;
			return updateResult;
		}


		private bool LoadModelFromExtractFile(Stream extract, SupportedProfiles extractType, ref ConcreteModel concreteModelResult, ref Assembly assembly, out string log)
		{
			bool valid = false;
			log = string.Empty;

			System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			try
			{
				ProfileManager.LoadAssembly(extractType, out assembly);
				if (assembly != null)
				{
					CIMModel cimModel = new CIMModel();
					CIMModelLoaderResult modelLoadResult = CIMModelLoader.LoadCIMXMLModel(extract, ProfileManager.Namespace, out cimModel);
					if (modelLoadResult.Success)
					{
						concreteModelResult = new ConcreteModel();
						ConcreteModelBuilder builder = new ConcreteModelBuilder();
						ConcreteModelBuildingResult modelBuildResult = builder.GenerateModel(cimModel, assembly, ProfileManager.Namespace, ref concreteModelResult);

						if (modelBuildResult.Success)
						{
							valid = true;
						}
						log = modelBuildResult.Report.ToString();
					}
					else
					{
						log = modelLoadResult.Report.ToString();
					}
				}
			}
			catch (Exception e)
			{
				log = e.Message;
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = culture;
			}
			return valid;
		}

		private bool DoTransformAndLoad(Assembly assembly, ConcreteModel concreteModel, SupportedProfiles extractType, out Delta nmsDelta, out string log)
		{
			nmsDelta = null;
			log = string.Empty;
			bool success = false;
			try
			{
				LogManager.Log(string.Format("Importing {0} data...", extractType), LogLevel.Info);

				switch (extractType)
				{
					case SupportedProfiles.AMIProfile:
						{
							// transformation to DMS delta					
							TransformAndLoadReport report = Importer.Importer.Instance.CreateNMSDelta(concreteModel);

							if (report.Success)
							{
								nmsDelta = Importer.Importer.Instance.NMSDelta;
								success = true;
							}
							else
							{
								success = false;
							}
							log = report.Report.ToString();
                            Importer.Importer.Instance.Reset();

							break;
						}
					default:
						{
							LogManager.Log(string.Format("Import of {0} data is NOT SUPPORTED.", extractType), LogLevel.Warning);
							break;
						}
				}

				return success;
			}
			catch (Exception ex)
			{
				LogManager.Log(string.Format("Import unsuccessful: {0}", ex.StackTrace), LogLevel.Error);
				return false;
			}
		}

	}
}

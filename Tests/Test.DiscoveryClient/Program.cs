using MediaToolkit.Core;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;

namespace Test.Discovery
{
	class ScreenCastServiceInputUpdater
	{
		
		private volatile bool running = false;

		public void Start(Type contractType, int refreshInterval, int maxResults)
		{
			if (running)
			{
				Console.WriteLine("Invalid state...");
				return;
			}

			Task.Run(() =>
			{
				var udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();
				udpDiscoveryEndpoint.EndpointBehaviors.Add(new WcfDiscoveryAddressCustomEndpointBehavior());

				DiscoveryClient discoveryClient = null;

				try
				{
					discoveryClient = new DiscoveryClient(udpDiscoveryEndpoint);

					var criteria = new FindCriteria(contractType);
					criteria.Duration = TimeSpan.FromSeconds(refreshInterval);
					criteria.MaxResults = maxResults;

					//discoveryClient.FindAsync(criteria, this);

					if (!running)
					{
						running = true;
					}

					while (running)
					{

						Console.WriteLine("discoveryClient.Find(...) BEGIN");

						var result = discoveryClient.Find(criteria);

						if (result != null)
						{
							List<ScreenCastServiceInput> inputs = new List<ScreenCastServiceInput>();

							//var endpoints = result.Endpoints;
							//if (endpoints == null || endpoints.Count == 0)
							//{
							//	Console.WriteLine("endpoints == null || endpoints.Count == 0");
							//	continue;
							//}

							
							foreach (var ep in result.Endpoints)
							{
								string address = ep.Address.ToString();
								string hostName = address;

								var extensions = ep.Extensions;
								if (extensions != null && extensions.Count > 0)
								{
									var hostElement = extensions.FirstOrDefault(el => el.Name == "HostName");
									if (hostElement != null)
									{
										hostName = hostElement.Value;// + " {" + address + "}";
									}
								}

								var input = new ScreenCastServiceInput
								{
									HostName = hostName,
									Address = address,
								};

								inputs.Add(input);
								//Console.WriteLine(hostName + " " + address);
							}

							InputsUpdated?.Invoke(inputs);

						}

						Console.WriteLine("discoveryClient.Find(...) END");
					}
				}
				finally
				{
					if (discoveryClient != null)
					{
						discoveryClient.Close();
						discoveryClient = null;
					}
				}
			});
		}


		public void Stop()
		{
			running = false;
		}

		public event Action<IEnumerable<ScreenCastServiceInput>> InputsUpdated;
	}

	

	public class ScreenCastServiceInput
	{
		public string Id { get; set; }
		public string HostName { get; set; }
		public string Address { get; set; }

	}

	class Program
	{
		static void Main(string[] args)
		{

			Dictionary<string, ScreenCastServiceInput> inputsDict = new Dictionary<string, ScreenCastServiceInput>();


			ScreenCastServiceInputUpdater inputUpdater = new ScreenCastServiceInputUpdater();

			inputUpdater.InputsUpdated += (inputs) =>
			{
				foreach(var i in inputs)
				{
					var addr = i.Address;
					if (inputsDict.ContainsKey(addr))
					{

					}
					else
					{
						inputsDict.Add(addr, i);
					}

					//Console.WriteLine(i.HostName + " " + i.Address);
	
				}

				List<string> toDelete = new List<string>();
				foreach(var key in inputsDict.Keys)
				{
					bool todel = true;
					foreach (var i in inputs)
					{
						if (key == i.Address)
						{
							todel = false;
							break;
						}
					}

					if (todel)
					{
						toDelete.Add(key);
					}
					
				}

				if(toDelete.Count > 0)
				{
					foreach(var i in toDelete)
					{
						if (inputsDict.ContainsKey(i))
						{
							inputsDict.Remove(i);
						}
						
					}
				}

				foreach (var key in inputsDict.Keys)
				{
					var input = inputsDict[key];

					Console.WriteLine(input.Address +  " " + input.HostName);

				}




			};

			inputUpdater.Start(typeof(IScreenCastService), 5, 10);

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();

			inputUpdater.Stop();

			//Task.Run(() =>
			//{
			//	var udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();
			//	udpDiscoveryEndpoint.EndpointBehaviors.Add(new WcfDiscoveryAddressCustomEndpointBehavior());


			//	DiscoveryClient discoveryClient = new DiscoveryClient(udpDiscoveryEndpoint);

			//	//discoveryClient.FindCompleted += DiscoveryClient_FindCompleted;
			//	//discoveryClient.FindProgressChanged += DiscoveryClient_FindProgressChanged;


			//	var criteria = new FindCriteria(typeof(IScreenCastService));
			//	criteria.Duration = TimeSpan.FromSeconds(5);
			//	criteria.MaxResults = 10;

			//	//discoveryClient.FindAsync(criteria, this);

			//	while (true)
			//	{
			//		Console.WriteLine("discoveryClient.Find(...) BEGIN");

			//		var result = discoveryClient.Find(criteria);

			//		if (result != null)
			//		{
			//			var endpoints = result.Endpoints;
			//			if(endpoints == null || endpoints.Count == 0)
			//			{
			//				Console.WriteLine("endpoints == null || endpoints.Count == 0");
			//				continue;
			//			}

			//			foreach (var ep in result.Endpoints)
			//			{
			//				string address = ep.Address.ToString();
			//				string hostName = address;

			//				var extensions = ep.Extensions;
			//				if (extensions != null && extensions.Count > 0)
			//				{
			//					var hostElement = extensions.FirstOrDefault(el => el.Name == "HostName");
			//					if (hostElement != null)
			//					{
			//						hostName = hostElement.Value;// + " {" + address + "}";
			//					}
			//				}

			//				Console.WriteLine(hostName + " " + address);

			//			}
			//		}

			//		Console.WriteLine("discoveryClient.Find(...) END");
			//	}
			//});

			//Console.WriteLine("Press any key to exit...");
			//Console.ReadKey();

		}

		static private void DiscoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
		{
			Console.WriteLine("DiscoveryClient_FindProgressChanged(...)");
		}

		static private void DiscoveryClient_FindCompleted(object sender, FindCompletedEventArgs e)
		{
			Console.WriteLine("DiscoveryClient_FindCompleted(...)");
			if (e.Error != null)
			{
				Console.WriteLine(e.Error);

				return;
			}



			if (!e.Cancelled)
			{
				var result = e.Result;
				if (result != null)
				{

					foreach (var ep in result.Endpoints)
					{
						string address = ep.Address.ToString();
						string hostName = address;

						var extensions = ep.Extensions;
						if (extensions != null && extensions.Count > 0)
						{
							var hostElement = extensions.FirstOrDefault(el => el.Name == "HostName");
							if (hostElement != null)
							{
								hostName = hostElement.Value;// + " {" + address + "}";
							}
						}

						Console.WriteLine(hostName+ " " + address);

					}
				}

			}
			else
			{
				Console.WriteLine("ScreenCastControl::FindCompleted(...) Cancelled");
				
			}

		}

	}
}

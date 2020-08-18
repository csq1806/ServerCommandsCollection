using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Management.Instrumentation;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigurationShell
{
	public static class NetworkHelper
	{
		public static List<NetworkAdapter> GetAllNetworks()
		{
			List<NetworkAdapter> networks = new List<NetworkAdapter>();
			var query = new ObjectQuery("SELECT * FROM Win32_NetworkAdapter");

			using (var searcher = new ManagementObjectSearcher(query))
			{
				var queryCollection = searcher.Get();

				foreach (var m in queryCollection)
				{
					networks.Add(new NetworkAdapter
					{
						Name = m["NetConnectionID"]?.ToString() ?? string.Empty,
						Description = m["Description"]?.ToString() ?? string.Empty,
						IsEnabled = Convert.ToBoolean(m["NetEnabled"] ?? false)
					});
				}

				return networks;
			}
		}

		public static bool IsNetworkExist(string networkName)
		{
			var query = new ObjectQuery($"SELECT * FROM Win32_NetworkAdapter");
			if (query == null) return false;
			using (var searcher = new ManagementObjectSearcher(query))
			{
				var queryCollection = searcher.Get();
				foreach (var m in queryCollection)
				{
					if ((m["NetConnectionID"]?.ToString() ?? string.Empty) == networkName) return true;
				}
			}
			return false;
		}
	}

	public struct NetworkAdapter
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsEnabled { get; set; }
	}
}

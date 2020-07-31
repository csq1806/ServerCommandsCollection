using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigureCLI.Commands
{
	class Connect2VPN : ICommand
	{
		public void AddCommand<T>(CommandLineApplication<T> app) where T : class
		{
			app.Command("vpn", config =>
			 {
				 var dns = config.Option("--dns", string.Empty, CommandOptionType.SingleOrNoValue);
				 var networkName = config.Option("-net", string.Empty, CommandOptionType.SingleValue);

				 var type = config.Option("--disconnect", string.Empty, CommandOptionType.NoValue);

				 var vpnName = config.Option("-name", string.Empty, CommandOptionType.SingleValue);
				 var userName = config.Option("-un", string.Empty, CommandOptionType.SingleValue);
				 var password = config.Option("-pw", string.Empty, CommandOptionType.SingleValue);
				 config.OnExecuteAsync(async token =>
				 {
					 if (dns.HasValue() && string.IsNullOrWhiteSpace(networkName.Value()))
					 {
						 Console.WriteLine("Set DNS must provide network name");
						 return 501;
					 }
					 if (type.HasValue()) return ExecuteDisconnectVPN(dns, networkName);
					 return ExecuteConnectVPN(dns, networkName, vpnName, userName, password);

				 });
			 });
		}

		private int ExecuteDisconnectVPN(CommandOption dns, CommandOption networkName)
		{
			using (var process = Process.Start("rasdial.exe", "/d"))
			{
				process.WaitForExit();
				if (process.ExitCode != 0) return process.ExitCode;
				if (dns.HasValue() && networkName.HasValue()) UnsetDNS(networkName.Value());
				return 0;
			}
		}

		private int ExecuteConnectVPN(CommandOption dns, CommandOption networkName, CommandOption vpnName, CommandOption userName, CommandOption password)
		{
			if (string.IsNullOrWhiteSpace(vpnName.Value()))
			{
				Console.WriteLine("Need VPN name");
				return 500;
			}

			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"\"{vpnName.Value()}\" {userName.Value()} {password.Value()}",
				FileName = "rasdial.exe"
			};
			using (Process process = Process.Start(startInfo))
			{
				process.WaitForExit();
				if (process.ExitCode != 0) return process.ExitCode;
				if (dns.HasValue() && networkName.HasValue()) SetDNS(dnsString: dns.Value() ?? "8.8.8.8", networkName: networkName.Value());
				return 0;
			}
		}



		public static NetworkInterface GetActiveEthernetOrWifiNetworkInterface(string networkName)
		{
			var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
				a => a.OperationalStatus == OperationalStatus.Up &&
				(a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
				a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork") &&
				a.Name == networkName);

			Console.WriteLine($"Get Network Interface, id:{Nic.Id}");
			return Nic;
		}

		private void SetDNS(string dnsString, string networkName)
		{
			string[] Dns = { dnsString };
			var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface(networkName);
			if (CurrentInterface == null) return;

			ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
			ManagementObjectCollection objMOC = objMC.GetInstances();
			foreach (ManagementObject objMO in objMOC)
			{
				if ((bool)objMO["IPEnabled"])
				{
					string settingId = objMO["SettingID"].ToString();
					Console.WriteLine($"Setting id:{settingId}");
					if (settingId == CurrentInterface.Id)
					{
						Console.WriteLine("Start setting DNS!");
						ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
						if (objdns != null)
						{
							objdns["DNSServerSearchOrder"] = Dns;
							objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
							Console.WriteLine("Setting DNS Done!");
						}
					}
				}
			}
		}
		private void UnsetDNS(string networkName)
		{
			var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface(networkName);
			if (CurrentInterface == null) return;

			ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
			ManagementObjectCollection objMOC = objMC.GetInstances();
			foreach (ManagementObject objMO in objMOC)
			{
				if ((bool)objMO["IPEnabled"])
				{
					string settingId = objMO["SettingID"].ToString();
					Console.WriteLine($"Setting id:{settingId}");
					if (settingId == CurrentInterface.Id)
					{
						Console.WriteLine("Start unsetting DNS!");
						ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
						if (objdns != null)
						{
							objdns["DNSServerSearchOrder"] = null;
							objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
							Console.WriteLine("Unsetting DNS Done!");
						}
					}
				}
			}
		}


	}
}

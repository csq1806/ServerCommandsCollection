using DotRas;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigureCLI.Commands
{
	class CreateVPN : ICommand
	{
		public void AddCommand<T>(CommandLineApplication<T> app) where T : class
		{
			app.Command("cvpn", config =>
			 {
				 var vpnType = config.Option("-l2tp", string.Empty, CommandOptionType.NoValue);
				 var vpnName = config.Option("-name", string.Empty, CommandOptionType.SingleValue);
				 var ip = config.Option("-ip", string.Empty, CommandOptionType.SingleValue);
				 var preSharedKey = config.Option("-prekey", string.Empty, CommandOptionType.SingleValue);
				 var userName = config.Option("-un", string.Empty, CommandOptionType.SingleOrNoValue);
				 var password = config.Option("-pw", string.Empty, CommandOptionType.SingleOrNoValue);
				 config.OnExecuteAsync(async token =>
				 {
					 if (!vpnType.HasValue())
					 {
						 Console.WriteLine("Set L2TP as VPN type");
						 return 502;
					 }
					 if (string.IsNullOrWhiteSpace(vpnName.Value()))
					 {
						 Console.WriteLine("Need VPN name for L2TP");
						 return 502;
					 }
					 if (string.IsNullOrWhiteSpace(ip.Value()))
					 {
						 Console.WriteLine("Need IP address for L2TP");
						 return 502;
					 }
					 if (string.IsNullOrWhiteSpace(preSharedKey.Value()))
					 {
						 Console.WriteLine("Need PreSharedKey for L2TP");
						 return 502;
					 }

					 using (RasPhoneBook allUsersPhoneBook = new RasPhoneBook())
					 {
						 try
						 {
							 allUsersPhoneBook.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));

							 //remove entry
							 RasEntry entry = allUsersPhoneBook.Entries.FirstOrDefault(p => p.Name == vpnName.Value());
							 if (entry != null)
							 {
								 return 504;
							 }

							 //create a new entry
							 entry = RasEntry.CreateVpnEntry(vpnName.Value(), ip.Value(), RasVpnStrategy.L2tpOnly, RasDevice.Create(vpnName.Value(), RasDeviceType.Vpn));
							 entry.EncryptionType = RasEncryptionType.Require;
							 entry.EntryType = RasEntryType.Vpn;
							 entry.Options.RequireDataEncryption = true;
							 entry.Options.UsePreSharedKey = true; // used only for IPSec - L2TP/IPsec VPN
							 entry.Options.UseLogOnCredentials = false;
							 entry.Options.RequireMSChap2 = true;
							 entry.Options.SecureFileAndPrint = true;
							 entry.Options.SecureClientForMSNet = true;
							 entry.Options.ReconnectIfDropped = false;

							 allUsersPhoneBook.Entries.Add(entry);
							 entry.UpdateCredentials(RasPreSharedKey.Client, preSharedKey.Value());
							 if (userName.HasValue() && password.HasValue() &&
								!string.IsNullOrWhiteSpace(userName.Value()) && !string.IsNullOrWhiteSpace(password.Value()))
								 entry.UpdateCredentials(new System.Net.NetworkCredential("transfinder_vpn", "Transfinder2503"));
						 }
						 catch (Exception ex)
						 {
							 Console.WriteLine(ex.Message);
							 return 503;
						 }
					 }
					 return 0;
				 });
			 });
		}
	}
}
